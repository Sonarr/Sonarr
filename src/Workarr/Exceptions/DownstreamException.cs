using System.Net;

namespace Workarr.Exceptions
{
    public class DownstreamException : WorkarrException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public DownstreamException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public DownstreamException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
