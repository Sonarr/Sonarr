using System;
using System.Net;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Common.Security;

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

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Method = request.Method.ToString();
            webRequest.UserAgent = _userAgentBuilder.GetUserAgent(request.UseSimplifiedUserAgent);
            webRequest.KeepAlive = request.ConnectionKeepAlive;
            webRequest.AllowAutoRedirect = request.AllowAutoRedirect;
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

            if (request.ContentData != null)
            {
                webRequest.ContentLength = request.ContentData.Length;
                using (var writeStream = webRequest.GetRequestStream())
                {
                    writeStream.Write(request.ContentData, 0, request.ContentData.Length);
                }
            }

            HttpWebResponse httpWebResponse;

            try
            {
                httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.SecureChannelFailure && OsInfo.IsWindows)
                {
                    SecurityProtocolPolicy.DisableTls12();
                }

                httpWebResponse = (HttpWebResponse)e.Response;

                if (httpWebResponse == null)
                {
                    throw;
                }
            }

            byte[] data = null;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    data = responseStream.ToBytes();
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
