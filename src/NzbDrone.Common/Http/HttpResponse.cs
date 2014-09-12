using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Http
{
    public class HttpResponse
    {
        public HttpResponse(HttpRequest request, HttpHeader headers, Byte[] binaryData, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = binaryData;
            StatusCode = statusCode;
        }

        public HttpResponse(HttpRequest request, HttpHeader headers, String content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Request = request;
            Headers = headers;
            ResponseData = Encoding.UTF8.GetBytes(content);
            _content = content;
            StatusCode = statusCode;
        }

        public HttpRequest Request { get; private set; }
        public HttpHeader Headers { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Byte[] ResponseData { get; private set; }

        private String _content;

        public String Content 
        {
            get
            {
                if (_content == null)
                {
                    _content = GetStringFromResponseData();
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

            if (HasHttpError)
            {
                result += Environment.NewLine + Content;
            }

            return result;
        }

        protected virtual String GetStringFromResponseData()
        {
            Encoding encoding = null;

            if (Headers.ContentType.IsNotNullOrWhiteSpace())
            {
                var charset = Headers.ContentType.ToLowerInvariant()
                    .Split(';', '=', ' ')
                    .SkipWhile(v => v != "charset")
                    .Skip(1).FirstOrDefault();

                if (charset.IsNotNullOrWhiteSpace())
                {
                    encoding = Encoding.GetEncoding(charset);
                }
            }

            if (encoding == null)
            {
                // TODO: Find encoding by Byte order mask.
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding.GetString(ResponseData);
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