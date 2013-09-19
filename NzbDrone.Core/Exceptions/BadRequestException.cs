using System.Net;

namespace NzbDrone.Core.Exceptions
{
    public class BadRequestException : DownstreamException
    {
        public BadRequestException(HttpStatusCode statusCode, string message) : base(statusCode, message)
        {
        }

        public BadRequestException(HttpStatusCode statusCode, string message, params object[] args) : base(statusCode, message, args)
        {
        }
    }
}
