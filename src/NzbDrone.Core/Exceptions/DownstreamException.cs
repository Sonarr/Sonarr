using System.Net;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class DownstreamException : NzbDroneException
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
