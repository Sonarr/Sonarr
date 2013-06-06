using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Exceptron.Client.fastJSON;

namespace Exceptron.Client
{
    public sealed class RestClient : IRestClient
    {
        public TResponse Put<TResponse>(string url, object content) where TResponse : class ,new()
        {

            if (content == null)
                throw new ArgumentNullException("content can not be null", "content");

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url can not be null or empty", "url");

            Trace.WriteLine("Attempting PUT to " + url);

            var json = JSON.Instance.ToJSON(content);

            var bytes = Encoding.UTF8.GetBytes(json);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            request.Accept = "application/json";

            var dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            string responseContent = string.Empty;

            try
            {
                var webResponse = request.GetResponse();
                responseContent = ReadResponse(webResponse);
                var response = JSON.Instance.ToObject<TResponse>(responseContent);

                return response;
            }
            catch (WebException e)
            {
                Trace.WriteLine(e.ToString());
                responseContent = ReadResponse(e.Response);
                throw new ExceptronApiException(e, responseContent);
            }
            finally
            {
                Trace.WriteLine(responseContent);
            }
        }


        public static string ReadResponse(WebResponse webResponse)
        {
            if (webResponse == null) return string.Empty;

            var responseStream = webResponse.GetResponseStream();

            if (responseStream == null) return string.Empty;
            
            var decodedStream = new StreamReader(responseStream, Encoding.GetEncoding(1252));
            return decodedStream.ReadToEnd();
        }
    }
}
