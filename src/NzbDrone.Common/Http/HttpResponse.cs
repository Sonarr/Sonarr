using System;
using System.IO;
using System.Net;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        public HttpResponse(HttpRequest request, HttpHeader headers, string content, HttpStatusCode statusCode)
        {
            Request = request;
            Headers = headers;
            Content = content;
            StatusCode = statusCode;
        }

        public HttpRequest Request { get; private set; }
        public HttpHeader Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }

        public bool HasHttpError
        {
            get
            {
                return (int)StatusCode >= 400;
            }
        }

        public override string ToString()
        {
            var result = string.Format("Res: [{0}] {1} : {2}.{3}", Request.Method, Request.Url, (int)StatusCode, StatusCode);

            if (HasHttpError)
            {
                result += Environment.NewLine + Content;
            }

            return result;
        }

        public Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(Content);
            writer.Flush();
            stream.Position = 0;
            return stream;
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