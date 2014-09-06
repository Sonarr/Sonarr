using System;
using System.Net;
using System.Security.Policy;
using NzbDrone.Common.Serializer;
using RestSharp;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        public HttpResponse(HttpRequest request, WebHeaderCollection headers, string content, HttpStatusCode statusCode)
        {
            Request = request;
            Headers = headers;
            Content = content;
            StatusCode = statusCode;
        }

        public HttpRequest Request { get; private set; }
        public WebHeaderCollection Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }


        public override string ToString()
        {
            return string.Format("Res: [{0}] {1} : {2} {3} {4}", Request.Method, Request.Url, StatusCode, Environment.NewLine, Content);
        }
    }

    public class HttpResponse<T> : HttpResponse where T : new()
    {
        public HttpResponse(HttpResponse response)
            : base(response.Request, response.Headers, response.Content, response.StatusCode)
        {
            Resource = Json.Deserialize<T>(response.Content);
        }

        public T Resource { get; private set; }
    }
}