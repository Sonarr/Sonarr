using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CurlSharp;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Http
{
    public class CurlHttpClient
    {
        private static Logger Logger = NzbDroneLogger.GetLogger(typeof(CurlHttpClient));
        private static readonly Regex ExpiryDate = new Regex(@"(expires=)([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public CurlHttpClient()
        {
            if (!CheckAvailability())
            {
                throw new ApplicationException("Curl failed to initialize.");
            }
        }

        public static bool CheckAvailability()
        {
            try
            {
                return CurlGlobalHandle.Instance.Initialize();
            }
            catch (Exception ex)
            {
                Logger.TraceException("Initializing curl failed", ex);
                return false;
            }
        }

        public HttpResponse GetResponse(HttpRequest httpRequest, HttpWebRequest webRequest)
        {
            lock (CurlGlobalHandle.Instance)
            {
                Stream responseStream = new MemoryStream();
                Stream headerStream = new MemoryStream();

                using (var curlEasy = new CurlEasy())
                {
                    curlEasy.AutoReferer = false;
                    curlEasy.WriteFunction = (b, s, n, o) =>
                    {
                        responseStream.Write(b, 0, s * n);
                        return s * n;
                    };
                    curlEasy.HeaderFunction = (b, s, n, o) =>
                    {
                        headerStream.Write(b, 0, s * n);
                        return s * n;
                    };

                    curlEasy.UserAgent = webRequest.UserAgent;
                    curlEasy.FollowLocation = webRequest.AllowAutoRedirect;
                    curlEasy.HttpGet = webRequest.Method == "GET";
                    curlEasy.Post = webRequest.Method == "POST";
                    curlEasy.Put = webRequest.Method == "PUT";
                    curlEasy.Url = webRequest.RequestUri.AbsoluteUri;

                    if (OsInfo.IsWindows)
                    {
                        curlEasy.CaInfo = "curl-ca-bundle.crt";
                    }

                    if (webRequest.CookieContainer != null)
                    {
                        curlEasy.Cookie = webRequest.CookieContainer.GetCookieHeader(webRequest.RequestUri);
                    }

                    if (!httpRequest.Body.IsNullOrWhiteSpace())
                    {
                        // TODO: This might not go well with encoding.
                        curlEasy.PostFieldSize = httpRequest.Body.Length;
                        curlEasy.SetOpt(CurlOption.CopyPostFields, httpRequest.Body);
                    }

                    // Yes, we have to keep a ref to the object to prevent corrupting the unmanaged state
                    using (var httpRequestHeaders = SerializeHeaders(webRequest))
                    {
                        curlEasy.HttpHeader = httpRequestHeaders;

                        var result = curlEasy.Perform();

                        if (result != CurlCode.Ok)
                        {
                            throw new WebException(string.Format("Curl Error {0} for Url {1}", result, curlEasy.Url));
                        }
                    }

                    var webHeaderCollection = ProcessHeaderStream(webRequest, headerStream);
                    var responseData = ProcessResponseStream(webRequest, responseStream, webHeaderCollection);

                    var httpHeader = new HttpHeader(webHeaderCollection);

                    return new HttpResponse(httpRequest, httpHeader, responseData, (HttpStatusCode)curlEasy.ResponseCode);
                }
            }
        }

        private CurlSlist SerializeHeaders(HttpWebRequest webRequest)
        {
            if (webRequest.SendChunked)
            {
                throw new NotSupportedException("Chunked transfer is not supported");
            }

            if (webRequest.ContentLength > 0)
            {
                webRequest.Headers.Add("Content-Length", webRequest.ContentLength.ToString());
            }

            if (webRequest.AutomaticDecompression.HasFlag(DecompressionMethods.GZip))
            {
                if (webRequest.AutomaticDecompression.HasFlag(DecompressionMethods.Deflate))
                {
                    webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                }
                else
                {
                    webRequest.Headers.Add("Accept-Encoding", "gzip");
                }
            }
            else
            {
                if (webRequest.AutomaticDecompression.HasFlag(DecompressionMethods.Deflate))
                {
                    webRequest.Headers.Add("Accept-Encoding", "deflate");
                }
            }


            var curlHeaders = new CurlSlist();
            for (int i = 0; i < webRequest.Headers.Count; i++)
            {
                curlHeaders.Append(webRequest.Headers.GetKey(i) + ": " + webRequest.Headers.Get(i));
            }

            curlHeaders.Append("Content-Type: " + webRequest.ContentType ?? string.Empty);

            return curlHeaders;
        }

        private WebHeaderCollection ProcessHeaderStream(HttpWebRequest webRequest, Stream headerStream)
        {
            headerStream.Position = 0;
            var headerData = headerStream.ToBytes();
            var headerString = Encoding.ASCII.GetString(headerData);

            var webHeaderCollection = new WebHeaderCollection();

            // following a redirect we could have two sets of headers, so only process the last one
            foreach (var header in headerString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Reverse())
            {
                if (!header.Contains(":")) break;
                webHeaderCollection.Add(header);
            }

            var setCookie = webHeaderCollection.Get("Set-Cookie");
            if (setCookie != null && setCookie.Length > 0 && webRequest.CookieContainer != null)
            {
                webRequest.CookieContainer.SetCookies(webRequest.RequestUri, FixSetCookieHeader(setCookie));
            }

            return webHeaderCollection;
        }

        private string FixSetCookieHeader(string setCookie)
        {
            // fix up the date if it was malformed
            var setCookieClean = ExpiryDate.Replace(setCookie, delegate(Match match)
                                                    {
                                                        string format = "ddd, dd-MMM-yyyy HH:mm:ss";
                                                        DateTime dt = Convert.ToDateTime(match.Groups[2].Value);
                                                        return match.Groups[1].Value + dt.ToUniversalTime().ToString(format) + " GMT";
                                                    });
            return setCookieClean;
        }

        private byte[] ProcessResponseStream(HttpWebRequest webRequest, Stream responseStream, WebHeaderCollection webHeaderCollection)
        {
            responseStream.Position = 0;

            if (responseStream.Length != 0 && webRequest.AutomaticDecompression != DecompressionMethods.None)
            {
                var encoding = webHeaderCollection["Content-Encoding"];
                if (encoding != null)
                {
                    if (webRequest.AutomaticDecompression.HasFlag(DecompressionMethods.GZip) && encoding.IndexOf("gzip") != -1)
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                        webHeaderCollection.Remove("Content-Encoding");
                    }
                    else if (webRequest.AutomaticDecompression.HasFlag(DecompressionMethods.Deflate) && encoding.IndexOf("deflate") != -1)
                    {
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

                        webHeaderCollection.Remove("Content-Encoding");
                    }
                }
            }

            return responseStream.ToBytes();

        }
    }

    internal sealed class CurlGlobalHandle : SafeHandle
    {
        public static readonly CurlGlobalHandle Instance = new CurlGlobalHandle();

        private bool _initialized;
        private bool _available;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private CurlGlobalHandle()
            : base(IntPtr.Zero, true)
        {

        }

        public bool Initialize()
        {
            lock (CurlGlobalHandle.Instance)
            {
                if (_initialized)
                    return _available;

                _initialized = true;
                _available = Curl.GlobalInit(CurlInitFlag.All) == CurlCode.Ok;

                return _available;
            }
        }

        protected override bool ReleaseHandle()
        {
            if (_initialized && _available)
            {
                Curl.GlobalCleanup();
                _available = false;
            }
            return true;
        }

        public override bool IsInvalid
        {
            get { return !_initialized || !_available; }
        }
    }
}
