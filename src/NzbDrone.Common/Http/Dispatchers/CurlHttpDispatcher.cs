using System;
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

namespace NzbDrone.Common.Http.Dispatchers
{
    public class CurlHttpDispatcher : IHttpDispatcher
    {
        private static readonly Regex ExpiryDate = new Regex(@"(expires=)([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(CurlHttpDispatcher));

        public static bool CheckAvailability()
        {
            try
            {
                return CurlGlobalHandle.Instance.Initialize();
            }
            catch (Exception ex)
            {
                _logger.TraceException("Initializing curl failed", ex);
                return false;
            }
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            if (!CheckAvailability())
            {
                throw new ApplicationException("Curl failed to initialize.");
            }

            if (request.NetworkCredential != null)
            {
                throw new NotImplementedException("Credentials not supported for curl dispatcher.");
            }

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
                    
                    curlEasy.Url = request.Url.AbsoluteUri;
                    switch (request.Method)
                    {
                        case HttpMethod.GET:
                            curlEasy.HttpGet = true;
                            break;

                        case HttpMethod.POST:
                            curlEasy.Post = true;
                            break;

                        case HttpMethod.PUT:
                            curlEasy.Put = true;
                            break;

                        default:
                            throw new NotSupportedException(string.Format("HttpCurl method {0} not supported", request.Method));
                    }
                    curlEasy.UserAgent = UserAgentBuilder.UserAgent;
                    curlEasy.FollowLocation = request.AllowAutoRedirect;

                    if (OsInfo.IsWindows)
                    {
                        curlEasy.CaInfo = "curl-ca-bundle.crt";
                    }

                    if (cookies != null)
                    {
                        curlEasy.Cookie = cookies.GetCookieHeader(request.Url);
                    }

                    if (!request.Body.IsNullOrWhiteSpace())
                    {
                        // TODO: This might not go well with encoding.
                        curlEasy.PostFieldSize = request.Body.Length;
                        curlEasy.SetOpt(CurlOption.CopyPostFields, request.Body);
                    }

                    // Yes, we have to keep a ref to the object to prevent corrupting the unmanaged state
                    using (var httpRequestHeaders = SerializeHeaders(request))
                    {
                        curlEasy.HttpHeader = httpRequestHeaders;

                        var result = curlEasy.Perform();

                        if (result != CurlCode.Ok)
                        {
                            throw new WebException(string.Format("Curl Error {0} for Url {1}", result, curlEasy.Url));
                        }
                    }

                    var webHeaderCollection = ProcessHeaderStream(request, cookies, headerStream);
                    var responseData = ProcessResponseStream(request, responseStream, webHeaderCollection);

                    var httpHeader = new HttpHeader(webHeaderCollection);

                    return new HttpResponse(request, httpHeader, responseData, (HttpStatusCode)curlEasy.ResponseCode);
                }
            }
        }

        private CurlSlist SerializeHeaders(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Accept-Encoding"))
            {
                request.Headers.Add("Accept-Encoding", "gzip");
            }

            if (request.Headers.ContentType == null)
            {
                request.Headers.ContentType = string.Empty;
            }

            var curlHeaders = new CurlSlist();
            foreach (var header in request.Headers)
            {
                curlHeaders.Append(header.Key + ": " + header.Value.ToString());
            }

            return curlHeaders;
        }

        private WebHeaderCollection ProcessHeaderStream(HttpRequest request, CookieContainer cookies, Stream headerStream)
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
            if (setCookie != null && setCookie.Length > 0 && cookies != null)
            {
                cookies.SetCookies(request.Url, FixSetCookieHeader(setCookie));
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

        private byte[] ProcessResponseStream(HttpRequest request, Stream responseStream, WebHeaderCollection webHeaderCollection)
        {
            responseStream.Position = 0;

            if (responseStream.Length != 0)
            {
                var encoding = webHeaderCollection["Content-Encoding"];
                if (encoding != null)
                {
                    if (encoding.IndexOf("gzip") != -1)
                    {
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                        webHeaderCollection.Remove("Content-Encoding");
                    }
                    else if (encoding.IndexOf("deflate") != -1)
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
