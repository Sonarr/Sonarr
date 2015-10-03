using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.TPL;

namespace NzbDrone.Common.Http
{
    public interface IHttpClient
    {
        HttpResponse Execute(HttpRequest request);
        void DownloadFile(string url, string fileName);
        HttpResponse Get(HttpRequest request);
        HttpResponse<T> Get<T>(HttpRequest request) where T : new();
        HttpResponse Head(HttpRequest request);
        HttpResponse Post(HttpRequest request);
        HttpResponse<T> Post<T>(HttpRequest request) where T : new();
    }

    public class HttpClient : IHttpClient
    {
        private readonly Logger _logger;
        private readonly IRateLimitService _rateLimitService;
        private readonly ICached<CookieContainer> _cookieContainerCache;
        private readonly ICached<bool> _curlTLSFallbackCache;
        private readonly List<IHttpRequestInterceptor> _requestInterceptors;

        public HttpClient(IEnumerable<IHttpRequestInterceptor> requestInterceptors, ICacheManager cacheManager, IRateLimitService rateLimitService, Logger logger)
        {
            _logger = logger;
            _rateLimitService = rateLimitService;
            _requestInterceptors = requestInterceptors.ToList();
            ServicePointManager.DefaultConnectionLimit = 12;

            _cookieContainerCache = cacheManager.GetCache<CookieContainer>(typeof(HttpClient));
            _curlTLSFallbackCache = cacheManager.GetCache<bool>(typeof(HttpClient), "curlTLSFallback");
        }

        public HttpResponse Execute(HttpRequest request)
        {
            foreach (var interceptor in _requestInterceptors)
            {
                request = interceptor.PreRequest(request);
            }

            if (request.RateLimit != TimeSpan.Zero)
            {
                _rateLimitService.WaitAndPulse(request.Url.Host, request.RateLimit);
            }

            _logger.Trace(request);

            var webRequest = (HttpWebRequest)WebRequest.Create(request.Url);

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Credentials = request.NetworkCredential;
            webRequest.Method = request.Method.ToString();
            webRequest.UserAgent = UserAgentBuilder.UserAgent;
            webRequest.KeepAlive = false;
            webRequest.AllowAutoRedirect = request.AllowAutoRedirect;
            webRequest.ContentLength = 0;

            var stopWatch = Stopwatch.StartNew();

            if (request.Headers != null)
            {
                AddRequestHeaders(webRequest, request.Headers);
            }

            PrepareRequestCookies(request, webRequest);

            var response = ExecuteRequest(request, webRequest);

            HandleResponseCookies(request, webRequest);

            stopWatch.Stop();

            _logger.Trace("{0} ({1:n0} ms)", response, stopWatch.ElapsedMilliseconds);

            if (!RuntimeInfoBase.IsProduction &&
                (response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.MovedPermanently ||
                response.StatusCode == HttpStatusCode.Found))
            {
                _logger.Error("Server requested a redirect to [" + response.Headers["Location"] + "]. Update the request URL to avoid this redirect.");
            }

            if (!request.SuppressHttpError && response.HasHttpError)
            {
                _logger.Warn("HTTP Error - {0}", response);

                if ((int)response.StatusCode == 429)
                {
                    throw new TooManyRequestsException(request, response);
                }
                else
                {
                    throw new HttpException(request, response);
                }
            }

            foreach (var interceptor in _requestInterceptors)
            {
                response = interceptor.PostResponse(response);
            }

            return response;
        }

        private void PrepareRequestCookies(HttpRequest request, HttpWebRequest webRequest)
        {
            lock (_cookieContainerCache)
            {
                var persistentCookieContainer = _cookieContainerCache.Get("container", () => new CookieContainer());

                if (request.Cookies.Count != 0)
                {
                    foreach (var pair in request.Cookies)
                    {
                        persistentCookieContainer.Add(new Cookie(pair.Key, pair.Value, "/", request.Url.Host)
                        {
                            Expires = DateTime.UtcNow.AddHours(1)
                        });
                    }
                }

                var requestCookies = persistentCookieContainer.GetCookies(request.Url);

                if (requestCookies.Count == 0 && !request.StoreResponseCookie)
                {
                    return;
                }

                if (webRequest.CookieContainer == null)
                {
                    webRequest.CookieContainer = new CookieContainer();
                }

                webRequest.CookieContainer.Add(requestCookies);
            }
        }

        private void HandleResponseCookies(HttpRequest request, HttpWebRequest webRequest)
        {
            if (!request.StoreResponseCookie)
            {
                return;
            }

            lock (_cookieContainerCache)
            {
                var persistentCookieContainer = _cookieContainerCache.Get("container", () => new CookieContainer());

                var cookies = webRequest.CookieContainer.GetCookies(request.Url);

                persistentCookieContainer.Add(cookies);
            }
        }

        private HttpResponse ExecuteRequest(HttpRequest request, HttpWebRequest webRequest)
        {
            if (OsInfo.IsMonoRuntime && webRequest.RequestUri.Scheme == "https")
            {
                if (!_curlTLSFallbackCache.Find(webRequest.RequestUri.Host))
                {
                    try
                    {
                        return ExecuteWebRequest(request, webRequest);
                    }
                    catch (Exception ex)
                    {
                        if (ex.ToString().Contains("The authentication or decryption has failed."))
                        {
                            _logger.Debug("https request failed in tls error for {0}, trying curl fallback.", webRequest.RequestUri.Host);

                            _curlTLSFallbackCache.Set(webRequest.RequestUri.Host, true);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (CurlHttpClient.CheckAvailability())
                {
                    return ExecuteCurlRequest(request, webRequest);
                }

                _logger.Trace("Curl not available, using default WebClient.");
            }
            
            return ExecuteWebRequest(request, webRequest);
        }

        private HttpResponse ExecuteCurlRequest(HttpRequest request, HttpWebRequest webRequest)
        {
            var curlClient = new CurlHttpClient();

            return curlClient.GetResponse(request, webRequest);
        }

        private HttpResponse ExecuteWebRequest(HttpRequest request, HttpWebRequest webRequest)
        {
            if (!request.Body.IsNullOrWhiteSpace())
            {
                var bytes = request.Headers.GetEncodingFromContentType().GetBytes(request.Body.ToCharArray());

                webRequest.ContentLength = bytes.Length;
                using (var writeStream = webRequest.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            HttpWebResponse httpWebResponse;

            try
            {
                httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
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

        public void DownloadFile(string url, string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                _logger.Debug("Downloading [{0}] to [{1}]", url, fileName);

                var stopWatch = Stopwatch.StartNew();
                var webClient = new GZipWebClient();
                webClient.Headers.Add(HttpRequestHeader.UserAgent, UserAgentBuilder.UserAgent);
                webClient.DownloadFile(url, fileName);
                stopWatch.Stop();
                _logger.Debug("Downloading Completed. took {0:0}s", stopWatch.Elapsed.Seconds);
            }
            catch (WebException e)
            {
                _logger.Warn("Failed to get response from: {0} {1}", url, e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.WarnException("Failed to get response from: " + url, e);
                throw;
            }
        }

        public HttpResponse Get(HttpRequest request)
        {
            request.Method = HttpMethod.GET;
            return Execute(request);
        }

        public HttpResponse<T> Get<T>(HttpRequest request) where T : new()
        {
            var response = Get(request);
            return new HttpResponse<T>(response);
        }

        public HttpResponse Head(HttpRequest request)
        {
            request.Method = HttpMethod.HEAD;
            return Execute(request);
        }

        public HttpResponse Post(HttpRequest request)
        {
            request.Method = HttpMethod.POST;
            return Execute(request);
        }

        public HttpResponse<T> Post<T>(HttpRequest request) where T : new()
        {
            var response = Post(request);
            return new HttpResponse<T>(response);
        }

        protected virtual void AddRequestHeaders(HttpWebRequest webRequest, HttpHeader headers)
        {
            foreach (var header in headers)
            {
                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Accept = header.Value.ToString();
                        break;
                    case "Connection":
                        webRequest.Connection = header.Value.ToString();
                        break;
                    case "Content-Length":
                        webRequest.ContentLength = Convert.ToInt64(header.Value);
                        break;
                    case "Content-Type":
                        webRequest.ContentType = header.Value.ToString();
                        break;
                    case "Date":
                        webRequest.Date = (DateTime)header.Value;
                        break;
                    case "Expect":
                        webRequest.Expect = header.Value.ToString();
                        break;
                    case "Host":
                        webRequest.Host = header.Value.ToString();
                        break;
                    case "If-Modified-Since":
                        webRequest.IfModifiedSince = (DateTime)header.Value;
                        break;
                    case "Range":
                        throw new NotImplementedException();
                        break;
                    case "Referer":
                        webRequest.Referer = header.Value.ToString();
                        break;
                    case "Transfer-Encoding":
                        webRequest.TransferEncoding = header.Value.ToString();
                        break;
                    case "User-Agent":
                        throw new NotSupportedException("User-Agent other than Sonarr not allowed.");
                    case "Proxy-Connection":
                        throw new NotImplementedException();
                        break;
                    default:
                        webRequest.Headers.Add(header.Key, header.Value.ToString());
                        break;
                }
            }
        }
    }
}