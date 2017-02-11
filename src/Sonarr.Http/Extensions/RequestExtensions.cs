using System;
using Nancy;

namespace Sonarr.Http.Extensions
{
    public static class RequestExtensions
    {
        public static bool IsApiRequest(this Request request)
        {
            return request.Path.StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFeedRequest(this Request request)
        {
            return request.Path.StartsWith("/feed/", StringComparison.InvariantCultureIgnoreCase);
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

        public static bool IsLoginRequest(this Request request)
        {
            return request.Path.Equals("/login", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsContentRequest(this Request request)
        {
            return request.Path.StartsWith("/Content/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool GetBooleanQueryParameter(this Request request, string parameter, bool defaultValue = false)
        {
            var parameterValue = request.Query[parameter];

            if (parameterValue.HasValue)
            {
                return bool.Parse(parameterValue.Value);
            }

            return defaultValue;
        }
    }
}
