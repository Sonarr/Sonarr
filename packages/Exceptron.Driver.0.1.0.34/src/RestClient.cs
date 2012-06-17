using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Exceptron.Driver.fastJSON;

namespace Exceptron.Driver
{
    public sealed class RestClient : IRestClient
    {
        public TResponse Put<TResponse>(string url, object content) where TResponse : class ,new()
        {

            if(content == null)
                throw new ArgumentNullException("content can not be null", "content");

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url can not be null or empty", "url");

            Trace.WriteLine("Attempting PUT to " + url);

            var json = JSON.Instance.ToJSON(content);

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            request.Accept = "application/json";

            var dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();
            var webResponse = request.GetResponse();

            var responseStream = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(1252));
            var responseString = responseStream.ReadToEnd();

            Trace.WriteLine(responseString);
            var response = JSON.Instance.ToObject<TResponse>(responseString);

            return response;

            /*   try
            {
                var dataStream = request.GetRequestStream();
                dataStream.Write(bytes, 0, bytes.Length);
                dataStream.Close();
                webResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                Trace.WriteLine("An Error has occurred while Doing HTTP PUT. " + ex);
                webResponse = ex.Response;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("An Error has occurred while Doing HTTP PUT. " + ex);
            }

            var response = new TResponse();

            if (webResponse != null && webResponse.ContentType.Contains("json"))
            {
                var responseStream = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(1252));
                var responseString = responseStream.ReadToEnd();

                Trace.WriteLine(responseString);
                response = JSON.Instance.ToObject<TResponse>(responseString);
            }

            if (response == null) response = new TResponse();

            return response;*/
        }
    }
}
