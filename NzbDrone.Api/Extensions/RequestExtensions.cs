using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;

namespace NzbDrone.Api.Extensions
{
    public static class RequestExtensions
    {
        public static bool IsApiRequest(this Request request)
        {
            return request.Path.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSignalRRequest(this Request request)
        {
            return request.Path.StartsWith("/signalr/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsLocalRequest(this Request request)
        {
            return (request.UserHostAddress.Equals("localhost") ||
                    request.UserHostAddress.Equals("127.0.0.1") ||
                    request.UserHostAddress.Equals("::1"));
        }
    }
}
