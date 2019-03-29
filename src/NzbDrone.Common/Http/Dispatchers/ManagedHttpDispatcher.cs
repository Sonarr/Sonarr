using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class ManagedHttpDispatcher : IHttpDispatcher
    {
        private readonly IHttpProxySettingsProvider _proxySettingsProvider;
        private readonly ICreateManagedWebProxy _createManagedWebProxy;
        private readonly IUserAgentBuilder _userAgentBuilder;

        public ManagedHttpDispatcher(IHttpProxySettingsProvider proxySettingsProvider, ICreateManagedWebProxy createManagedWebProxy, IUserAgentBuilder userAgentBuilder)
        {
            _proxySettingsProvider = proxySettingsProvider;
            _createManagedWebProxy = createManagedWebProxy;
            _userAgentBuilder = userAgentBuilder;
        }

        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create((Uri)request.Url);

            if (PlatformInfo.IsMono)
            {
                // On Mono GZipStream/DeflateStream leaks memory if an exception is thrown, use an intermediate buffer in that case.
                webRequest.AutomaticDecompression = DecompressionMethods.None;
                webRequest.Headers.Add("Accept-Encoding", "gzip");
            }
            else
            {
                // Deflate is not a standard and could break depending on implementation.
                // we should just stick with the more compatible Gzip
                //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
                webRequest.AutomaticDecompression = DecompressionMethods.GZip;
            }
            
            webRequest.Method = request.Method.ToString();
            webRequest.UserAgent = _userAgentBuilder.GetUserAgent(request.UseSimplifiedUserAgent);
            webRequest.KeepAlive = request.ConnectionKeepAlive;
            webRequest.AllowAutoRedirect = false;
            webRequest.CookieContainer = cookies;

            if (request.RequestTimeout != TimeSpan.Zero)
            {
                webRequest.Timeout = (int)Math.Ceiling(request.RequestTimeout.TotalMilliseconds);
            }

            AddProxy(webRequest, request);

            if (request.Headers != null)
            {
                AddRequestHeaders(webRequest, request.Headers);
            }

            HttpWebResponse httpWebResponse;

            try
            {
                if (request.ContentData != null)
                {
                    webRequest.ContentLength = request.ContentData.Length;
                    using (var writeStream = webRequest.GetRequestStream())
                    {
                        writeStream.Write(request.ContentData, 0, request.ContentData.Length);
                    }
                }

                httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                httpWebResponse = (HttpWebResponse)e.Response;

                if (httpWebResponse == null)
                {
                    // The default messages for WebException on mono are pretty horrible.
                    if (e.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        throw new WebException($"DNS Name Resolution Failure: '{webRequest.RequestUri.Host}'", e.Status);
                    }
                    else if (e.ToString().Contains("TLS Support not"))
                    {
                        throw new TlsFailureException(webRequest, e);
                    }
                    else if (e.ToString().Contains("The authentication or decryption has failed."))
                    {
                        throw new TlsFailureException(webRequest, e);
                    }
                    else if (OsInfo.IsNotWindows)
                    {
                        throw new WebException($"{e.Message}: '{webRequest.RequestUri}'", e, e.Status, e.Response);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            byte[] data = null;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null && responseStream != Stream.Null)
                {
                    try
                    {
                        data = responseStream.ToBytes();

                        if (PlatformInfo.IsMono && httpWebResponse.ContentEncoding == "gzip")
                        {
                            using (var compressedStream = new MemoryStream(data))
                            using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
                            using (var decompressedStream = new MemoryStream())
                            {
                                gzip.CopyTo(decompressedStream);
                                data = decompressedStream.ToArray();
                            }

                            httpWebResponse.Headers.Remove("Content-Encoding");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new WebException("Failed to read complete http response", ex, WebExceptionStatus.ReceiveFailure, httpWebResponse);
                    }
                }
            }

            return new HttpResponse(request, new HttpHeader(httpWebResponse.Headers), data, httpWebResponse.StatusCode);
        }

        protected virtual void AddProxy(HttpWebRequest webRequest, HttpRequest request)
        {
            var proxySettings = _proxySettingsProvider.GetProxySettings(request);
            if (proxySettings != null)
            {
                webRequest.Proxy = _createManagedWebProxy.GetWebProxy(proxySettings);
            }
        }

        protected virtual void AddRequestHeaders(HttpWebRequest webRequest, HttpHeader headers)
        {
            foreach (var header in headers)
            {
                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Accept = header.Value;
                        break;
                    case "Connection":
                        webRequest.Connection = header.Value;
                        break;
                    case "Content-Length":
                        webRequest.ContentLength = Convert.ToInt64(header.Value);
                        break;
                    case "Content-Type":
                        webRequest.ContentType = header.Value;
                        break;
                    case "Date":
                        webRequest.Date = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Expect":
                        webRequest.Expect = header.Value;
                        break;
                    case "Host":
                        webRequest.Host = header.Value;
                        break;
                    case "If-Modified-Since":
                        webRequest.IfModifiedSince = HttpHeader.ParseDateTime(header.Value);
                        break;
                    case "Range":
                        throw new NotImplementedException();
                    case "Referer":
                        webRequest.Referer = header.Value;
                        break;
                    case "Transfer-Encoding":
                        webRequest.TransferEncoding = header.Value;
                        break;
                    case "User-Agent":
                        throw new NotSupportedException("User-Agent other than Sonarr not allowed.");
                    case "Proxy-Connection":
                        throw new NotImplementedException();
                    default:
                        webRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }
    }
}
