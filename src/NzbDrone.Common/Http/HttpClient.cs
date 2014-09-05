using System.IO;
using System.Net;
using NLog;

namespace NzbDrone.Common.Http
{
    public class HttpClient
    {
        private readonly Logger _logger;

        public HttpClient(Logger logger)
        {
            _logger = logger;
        }

        public HttpResponse Exetcute(HttpRequest request)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(request.Url);
            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            webRequest.Credentials = request.Credential;
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

            using (var response = (HttpWebResponse)webRequest.GetResponse())
            {
                var content = string.Empty;

                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }

                return new HttpResponse(response.Headers, content, response.StatusCode);
            }
        }


        /*

                public HttpResponse<T> Execute<T>(HttpRequest httpRequest, bool supressResponseStatus = false) where T : new()
                {

                    var


                    var restSharpRequest = new RestSharp.RestRequest(httpRequest.Url, ConvertToRestsharpMethod(httpRequest.Method));
                    restSharpRequest.AddBody(httpRequest.Body);

                    var response = ExecuteWithRestSharp<T>(restSharpRequest);

                    if (!supressResponseStatus)
                    {
                        ValidateStatusCode(httpRequest, response);
                    }

                    var headers = response.Headers.ToDictionary(c => c.Name, c => c.Value.ToString());
                    var restResponse = new HttpResponse<T>(headers, response.Data, response.StatusCode);

                    return restResponse;
                }

        */

    }
}