using System.IO;
using System.Net;
using NLog;

namespace NzbDrone.Common.Http
{
    public interface IHttpClient
    {
        HttpResponse Exetcute(HttpRequest request);
        
        HttpResponse Get(HttpRequest request);
        HttpResponse<T> Get<T>(HttpRequest request) where T : new();
    }

    public class HttpClient : IHttpClient
    {
        private readonly Logger _logger;

        public HttpClient(Logger logger)
        {
            _logger = logger;
        }

        public HttpResponse Exetcute(HttpRequest request)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(request.Url);

            // Deflate is not a standard and could break depending on implementation.
            // we should just stick with the more compatible Gzip
            //http://stackoverflow.com/questions/8490718/how-to-decompress-stream-deflated-with-java-util-zip-deflater-in-net
            webRequest.AutomaticDecompression = DecompressionMethods.GZip;

            webRequest.Credentials = request.NetworkCredential;
            webRequest.Method = request.Method.ToString();
            webRequest.KeepAlive = false;
            webRequest.ServicePoint.Expect100Continue = false;

            if (!request.Body.IsNullOrWhiteSpace())
            {
                var bytes = new byte[request.Body.Length * sizeof(char)];
                System.Buffer.BlockCopy(request.Body.ToCharArray(), 0, bytes, 0, bytes.Length);

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

            var content = string.Empty;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        content = reader.ReadToEnd();
                    }
                }
            }

            var response = new HttpResponse(httpWebResponse.Headers, content, httpWebResponse.StatusCode);

            if (!supressHttpError && (int)response.StatusCode >= 400)
            {
                throw new HttpException(request, response);
            }

            return response;
        }

        public HttpResponse Get(HttpRequest request)
        {
            request.Method = HttpMethod.GET;
            return Exetcute(request);
        }

        public HttpResponse<T> Get<T>(HttpRequest request) where T : new()
        {
            var response = Get(request);
            return new HttpResponse<T>(response.Headers, response.Content, response.StatusCode);
        }

    }
}