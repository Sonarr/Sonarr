using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public interface IHttpClient
    {
        HttpResponse Execute(HttpRequest request);
        void DownloadFile(string url, string fileName);
        HttpResponse Get(HttpRequest request);
        HttpResponse<T> Get<T>(HttpRequest request) where T : new();
        HttpResponse Head(HttpRequest request);
    }

    public class HttpClient : IHttpClient
    {
        private readonly Logger _logger;
        private readonly string _userAgent;

        public HttpClient(Logger logger)
        {
            _logger = logger;
            _userAgent = String.Format("NzbDrone {0}", BuildInfo.Version);
            ServicePointManager.DefaultConnectionLimit = 12;
        }

        public HttpResponse Execute(HttpRequest request)
        {
            _logger.Trace(request);

            var webRequest = (HttpWebRequest)WebRequest.Create(request.Url);

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Credentials = request.NetworkCredential;
            webRequest.Method = request.Method.ToString();
            webRequest.KeepAlive = false;

            if (!RuntimeInfoBase.IsProduction)
            {
                webRequest.AllowAutoRedirect = false;
            }

            var stopWatch = Stopwatch.StartNew();

            if (!request.Body.IsNullOrWhiteSpace())
            {
                var bytes = new byte[request.Body.Length * sizeof(char)];
                Buffer.BlockCopy(request.Body.ToCharArray(), 0, bytes, 0, bytes.Length);

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
            }

            Byte[] data = null;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    data = responseStream.ToBytes();
                }
            }

            stopWatch.Stop();

            var response = new HttpResponse(request, new HttpHeader(httpWebResponse.Headers), data, httpWebResponse.StatusCode);
            _logger.Trace("{0} ({1:n0} ms)", response, stopWatch.ElapsedMilliseconds);

            if (!RuntimeInfoBase.IsProduction &&
                (response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.MovedPermanently))
            {
                throw new Exception("Server requested a redirect to [" + response.Headers["Location"] + "]. Update the request URL to avoid this redirect.");
            }

            if (!request.SuppressHttpError && response.HasHttpError)
            {
                _logger.Warn("HTTP Error - {0}", response);
                throw new HttpException(request, response);
            }

            return response;
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
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _userAgent);
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

    }
}