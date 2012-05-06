using System;
using System.IO;
using System.Net;
using System.Text;

namespace Exceptron.Driver
{
    internal class RestClient
    {
        internal virtual string Put(string url, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            request.Accept = "application/json";

            var dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse webResponse;

            try
            {
                webResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                webResponse = ex.Response;
            }

            var responseStream = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(1252));

            var responseString = responseStream.ReadToEnd();

            return responseString;
        }
    }
}
