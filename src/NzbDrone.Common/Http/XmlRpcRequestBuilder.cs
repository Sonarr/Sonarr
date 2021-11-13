using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.Http
{
    public class XmlRpcRequestBuilder : HttpRequestBuilder
    {
        public static string XmlRpcContentType = "text/xml";

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(XmlRpcRequestBuilder));

        public string XmlMethod { get; private set; }
        public List<object> XmlParameters { get; private set; }

        public XmlRpcRequestBuilder(string baseUrl)
            : base(baseUrl)
        {
            Method = HttpMethod.Post;
            XmlParameters = new List<object>();
        }

        public XmlRpcRequestBuilder(bool useHttps, string host, int port, string urlBase = null)
            : this(BuildBaseUrl(useHttps, host, port, urlBase))
        {
        }

        public override HttpRequestBuilder Clone()
        {
            var clone = base.Clone() as XmlRpcRequestBuilder;
            clone.XmlParameters = new List<object>(XmlParameters);
            return clone;
        }

        public XmlRpcRequestBuilder Call(string method, params object[] parameters)
        {
            var clone = Clone() as XmlRpcRequestBuilder;
            clone.XmlMethod = method;
            clone.XmlParameters = parameters.ToList();
            return clone;
        }

        protected override void Apply(HttpRequest request)
        {
            base.Apply(request);

            request.Headers.ContentType = XmlRpcContentType;

            var methodCallElements = new List<XElement> { new XElement("methodName", XmlMethod) };

            if (XmlParameters.Any())
            {
                var argElements = XmlParameters.Select(x => new XElement("param", ConvertParameter(x))).ToList();
                var paramsElement = new XElement("params", argElements);
                methodCallElements.Add(paramsElement);
            }

            var message = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("methodCall", methodCallElements));

            var body = message.ToString();

            Logger.Debug($"Executing remote method: {XmlMethod}");

            Logger.Trace($"methodCall {XmlMethod} body:\n{body}");

            request.SetContent(body);
        }

        private static XElement ConvertParameter(object value)
        {
            XElement data;

            if (value is string s)
            {
                data = new XElement("string", s);
            }
            else if (value is List<string> l)
            {
                data = new XElement("array", new XElement("data", l.Select(x => new XElement("value", new XElement("string", x)))));
            }
            else if (value is int i)
            {
                data = new XElement("int", i);
            }
            else if (value is byte[] bytes)
            {
                data = new XElement("base64", Convert.ToBase64String(bytes));
            }
            else
            {
                throw new InvalidOperationException($"Unhandled argument type {value.GetType().Name}");
            }

            return new XElement("value", data);
        }
    }
}
