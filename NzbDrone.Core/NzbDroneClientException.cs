using System.Net;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core
{
    public class NzbDroneClientException : NzbDroneException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public NzbDroneClientException(HttpStatusCode statusCode, string message, params object[] args) : base(message, args)
        {
            StatusCode = statusCode;
        }

        public NzbDroneClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
