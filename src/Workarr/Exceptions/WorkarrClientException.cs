using System.Net;

namespace Workarr.Exceptions
{
    public class WorkarrClientException : WorkarrException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public WorkarrClientException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public WorkarrClientException(HttpStatusCode statusCode, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
            StatusCode = statusCode;
        }

        public WorkarrClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
