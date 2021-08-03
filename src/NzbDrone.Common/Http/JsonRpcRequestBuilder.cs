using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class JsonRpcRequestBuilder : HttpRequestBuilder
    {
        public static HttpAccept JsonRpcHttpAccept = new HttpAccept("application/json-rpc, application/json");
        public static string JsonRpcContentType = "application/json";

        public string JsonMethod { get; private set; }
        public List<object> JsonParameters { get; private set; }

        public JsonRpcRequestBuilder(string baseUrl)
            : base(baseUrl)
        {
            Method = HttpMethod.POST;
            JsonParameters = new List<object>();
        }

        public JsonRpcRequestBuilder(string baseUrl, string method, IEnumerable<object> parameters)
            : base(baseUrl)
        {
            Method = HttpMethod.POST;
            JsonMethod = method;
            JsonParameters = parameters.ToList();
        }

        public override HttpRequestBuilder Clone()
        {
            var clone = base.Clone() as JsonRpcRequestBuilder;
            clone.JsonParameters = new List<object>(JsonParameters);
            return clone;
        }

        public JsonRpcRequestBuilder Call(string method, params object[] parameters)
        {
            var clone = Clone() as JsonRpcRequestBuilder;
            clone.JsonMethod = method;
            clone.JsonParameters = parameters.ToList();
            return clone;
        }

        protected override void Apply(HttpRequest request)
        {
            base.Apply(request);

            request.Headers.ContentType = JsonRpcContentType;

            var parameterData = new object[JsonParameters.Count];
            var parameterSummary = new string[JsonParameters.Count];

            for (var i = 0; i < JsonParameters.Count; i++)
            {
                ConvertParameter(JsonParameters[i], out parameterData[i], out parameterSummary[i]);
            }

            var message = new Dictionary<string, object>();
            message["jsonrpc"] = "2.0";
            message["method"] = JsonMethod;
            message["params"] = parameterData;
            message["id"] = CreateNextId();

            request.SetContent(message.ToJson());

            if (request.ContentSummary == null)
            {
                request.ContentSummary = string.Format("{0}({1})", JsonMethod, string.Join(", ", parameterSummary));
            }
        }

        private void ConvertParameter(object value, out object data, out string summary)
        {
            if (value is byte[])
            {
                data = Convert.ToBase64String(value as byte[]);
                summary = string.Format("[blob {0} bytes]", (value as byte[]).Length);
            }
            else if (value is Array && ((Array)value).Length > 0)
            {
                data = value;
                summary = "[...]";
            }
            else
            {
                data = value;
                summary = JsonConvert.SerializeObject(data);
            }
        }

        public string CreateNextId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}
