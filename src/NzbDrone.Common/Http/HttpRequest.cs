using System;
using System.Net;

namespace NzbDrone.Common.Http
{
    public class HttpRequest
    {
        public HttpRequest(string url)
        {
            UriBuilder = new UriBuilder(url);
            Headers = new HttpHeader();
            Headers.Accept = "application/json";
        }

        public UriBuilder UriBuilder { get; private set; }

        public Uri Url
        {
            get
            {
                return UriBuilder.Uri;
            }
        }

        public HttpMethod Method { get; set; }
        public HttpHeader Headers { get; set; }
        public string Body { get; set; }
        public NetworkCredential NetworkCredential { get; set; }
        public bool SuppressHttpError { get; set; }

        public override string ToString()
        {
            if (Body == null)
            {
                return string.Format("Req: [{0}] {1}", Method, Url);
            }

            return string.Format("Req: [{0}] {1} {2} {3}", Method, Url, Environment.NewLine, Body);
        }
    }
}