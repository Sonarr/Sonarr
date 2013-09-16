using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Core.Exceptions
{
    public static class StatusCodeToExceptions
    {
        public static void VerifyStatusCode(this HttpStatusCode statusCode, string message = null)
        {
            if (String.IsNullOrEmpty(message))
            {
                message = statusCode.ToString();
            }

            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(statusCode, message);

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
