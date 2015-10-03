using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class JsonRpcRequestBuilder : HttpRequestBuilder
    {
        public string Method { get; private set; }
        public List<object> Parameters { get; private set; }

        public JsonRpcRequestBuilder(string baseUri, string method, IEnumerable<object> parameters)
            : base (baseUri)
        {
            Method = method;
            Parameters = parameters.ToList();
        }

        public override HttpRequest Build(string path)
        {
            var request = base.Build(path);
            request.Method = HttpMethod.POST;
            request.Headers.Accept = "application/json-rpc, application/json";
            request.Headers.ContentType = "application/json-rpc";

            var message = new Dictionary<string, object>();
            message["jsonrpc"] = "2.0";
            message["method"] = Method;
            message["params"] = Parameters;
            message["id"] = CreateNextId();

            request.Body = message.ToJson();

            return request;
        }

        public string CreateNextId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}
