using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Http
{
    public class HttpRequest
    {
        public HttpRequest(string url, HttpAccept httpAccept = null)
        {
            Url = new HttpUri(url);
            Headers = new HttpHeader();
            AllowAutoRedirect = true;
            Cookies = new Dictionary<string, string>();
            
            if (!RuntimeInfoBase.IsProduction)
            {
                AllowAutoRedirect = false;
            }

            if (httpAccept != null)
            {
                Headers.Accept = httpAccept.Value;
            }
        }

        public HttpUri Url { get; set; }
        public HttpMethod Method { get; set; }
        public HttpHeader Headers { get; set; }
        public byte[] ContentData { get; set; }
        public string ContentSummary { get; set; }
        public bool SuppressHttpError { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public bool ConnectionKeepAlive { get; set; }
        public bool LogResponseContent { get; set; }
        public Dictionary<string, string> Cookies { get; private set; }
        public bool StoreResponseCookie { get; set; }
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan RateLimit { get; set; }
        public HttpRequestProxySettings Proxy {get; set;}

        public override string ToString()
        {
            if (ContentSummary == null)
            {
                return string.Format("Req: [{0}] {1}", Method, Url);
            }
            else
            {
                return string.Format("Req: [{0}] {1}: {2}", Method, Url, ContentSummary);
            }
        }

        public void SetContent(byte[] data)
        {
            ContentData = data;
        }

        public void SetContent(string data)
        {
            var encoding = HttpHeader.GetEncodingFromContentType(Headers.ContentType);
            ContentData = encoding.GetBytes(data);
        }
    }
}