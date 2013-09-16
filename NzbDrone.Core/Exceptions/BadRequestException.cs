using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NzbDrone.Common.Exceptions;

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
