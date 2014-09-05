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
        }

        public Uri Url { get; private set; }
        public HttpMethod Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public NetworkCredential Credential { get; set; }
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