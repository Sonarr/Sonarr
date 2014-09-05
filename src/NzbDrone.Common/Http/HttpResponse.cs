using System.Net;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        public HttpResponse(WebHeaderCollection headers, string content, HttpStatusCode statusCode)
        {
            Headers = headers;
            Content = content;
            StatusCode = statusCode;
        }

        public WebHeaderCollection Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }
    }

    public class HttpResponse<T> : HttpResponse where T : new()
    {
        public HttpResponse(WebHeaderCollection headers, string content, HttpStatusCode statusCode)
            : base(headers, content, statusCode)
        {
            Resource = Json.Deserialize<T>(content);
        }

        public T Resource { get; private set; }
    }
}