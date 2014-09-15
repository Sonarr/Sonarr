using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class JsonRpcRequestBuilder : HttpRequestBuilder
    {
        public String Method { get; private set; }
        public List<Object> Parameters { get; private set; }

        public JsonRpcRequestBuilder(String baseUri, String method, Object[] parameters)
            : base (baseUri)
        {
            Method = method;
            Parameters = parameters.ToList();
        }

        public override HttpRequest Build(String path)
        {
            var request = base.Build(path);
            request.Method = HttpMethod.POST;
            request.Headers.Accept = "application/json-rpc, application/json";
            request.Headers.ContentType = "application/json-rpc";

            var message = new Dictionary<String, Object>();
            message["jsonrpc"] = "2.0";
            message["method"] = Method;
            message["params"] = Parameters;
            message["id"] = CreateNextId();

            request.Body = message.ToJson();

            return request;
        }

        public String CreateNextId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}
