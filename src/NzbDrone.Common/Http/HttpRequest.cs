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
        public HttpRequest(string uri, HttpAccept httpAccept = null)
        {
            UrlBuilder = new UriBuilder(uri);
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

        public UriBuilder UrlBuilder { get; private set; }
        public Uri Url { get { return UrlBuilder.Uri; } }
        public HttpMethod Method { get; set; }
        public HttpHeader Headers { get; set; }
        public byte[] ContentData { get; set; }
        public string ContentSummary { get; set; }
        public NetworkCredential NetworkCredential { get; set; }
        public bool SuppressHttpError { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public Dictionary<string, string> Cookies { get; private set; }
        public bool StoreResponseCookie { get; set; }
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan RateLimit { get; set; }

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