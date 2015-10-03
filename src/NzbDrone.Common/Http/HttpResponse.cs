using System;
using System.Net;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        public HttpResponse(HttpRequest request, HttpHeader headers, byte[] binaryData, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = binaryData;
            StatusCode = statusCode;
        }

        public HttpResponse(HttpRequest request, HttpHeader headers, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = Headers.GetEncodingFromContentType().GetBytes(content);
            _content = content;
            StatusCode = statusCode;
        }

        public HttpRequest Request { get; private set; }
        public HttpHeader Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public byte[] ResponseData { get; private set; }

        private string _content;

        public string Content 
        {
            get
            {
                if (_content == null)
                {
                    _content = Headers.GetEncodingFromContentType().GetString(ResponseData);
                }

                return _content;
            }
        }


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

            if (HasHttpError && !Headers.ContentType.Equals("text/html", StringComparison.InvariantCultureIgnoreCase))
            {
                result += Environment.NewLine + Content;
            }

            return result;
        }
    }


    public class HttpResponse<T> : HttpResponse where T : new()
    {
        public HttpResponse(HttpResponse response)
            : base(response.Request, response.Headers, response.ResponseData, response.StatusCode)
        {
            Resource = Json.Deserialize<T>(response.Content);
        }

        public T Resource { get; private set; }
    }
}