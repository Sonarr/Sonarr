using System;
using System.Net;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http.Dispatchers
{
    public class ManagedHttpDispatcher : IHttpDispatcher
    {
        public HttpResponse GetResponse(HttpRequest request, CookieContainer cookies)
        {
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
            webRequest.CookieContainer = cookies;

            if (request.Headers != null)
            {
                AddRequestHeaders(webRequest, request.Headers);
            }

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
                    default:
                        webRequest.Headers.Add(header.Key, header.Value.ToString());
                        break;
                }
            }
        }
    }
}
