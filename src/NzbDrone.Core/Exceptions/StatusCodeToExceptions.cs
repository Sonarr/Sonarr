using System;
using System.Net;

namespace NzbDrone.Core.Exceptions
{
    public static class StatusCodeToExceptions
    {
        public static void VerifyStatusCode(this HttpStatusCode statusCode, string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = statusCode.ToString();
            }

            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(message);

                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException(message);

                case HttpStatusCode.PaymentRequired:
                    throw new DownstreamException(statusCode, message);

                case HttpStatusCode.InternalServerError:
                    throw new DownstreamException(statusCode, message);
            }
        }
    }
}
