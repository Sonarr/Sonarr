using System;
using System.Net;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Exceptions
{
    public class NzbDroneClientException : NzbDroneException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public NzbDroneClientException(HttpStatusCode statusCode, string message, params object[] args)
            : base(message, args)
        {
            StatusCode = statusCode;
        }

        public NzbDroneClientException(HttpStatusCode statusCode, string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
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
