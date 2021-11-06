using System;
using System.Net;

namespace Sonarr.Http.Exceptions
{
    public abstract class ApiException : Exception
    {
        public object Content { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        protected ApiException(HttpStatusCode statusCode, object content = null)
            : base(GetMessage(statusCode, content))
        {
            StatusCode = statusCode;
            Content = content;
        }

        private static string GetMessage(HttpStatusCode statusCode, object content)
        {
            var result = statusCode.ToString();

            if (content != null)
            {
                result = $"{result}: {content}";
            }

            return result;
        }
    }
}
