using System;
using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpRequest
    {
        public HttpRequest(string url)
        {
            Url = new Uri(url);
            Headers = new Dictionary<string, string>();
        }

        public Uri Url { get; private set; }
        public HttpMethod Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
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

    public class JsonHttpRequest : HttpRequest
    {
        public JsonHttpRequest(string url, object body)
            : base(url)
        {
            Headers["ContentType"] = "application/json";
            Body = body.ToJson();
        }
    }

}