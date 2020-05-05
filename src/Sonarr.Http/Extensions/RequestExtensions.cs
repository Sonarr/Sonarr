using System;
using System.Linq;
using System.Net;
using Nancy;
using NzbDrone.Common.Extensions;

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

        public static bool IsSharedContentRequest(this Request request)
        {
            return request.Path.StartsWith("/MediaCover/", StringComparison.InvariantCultureIgnoreCase) ||
                   request.Path.StartsWith("/Content/Images/", StringComparison.InvariantCultureIgnoreCase);
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

        public static int GetIntegerQueryParameter(this Request request, string parameter, int defaultValue = 0)
        {
            var parameterValue = request.Query[parameter];

            if (parameterValue.HasValue)
            {
                return int.Parse(parameterValue.Value);
            }

            return defaultValue;
        }

        public static int? GetNullableIntegerQueryParameter(this Request request, string parameter, int? defaultValue = null)
        {
            var parameterValue = request.Query[parameter];

            if (parameterValue.HasValue)
            {
                return int.Parse(parameterValue.Value);
            }

            return defaultValue;
        }

        public static string GetRemoteIP(this NancyContext context)
        {
            if (context == null || context.Request == null)
            {
                return "Unknown";
            }

            var remoteAddress = context.Request.UserHostAddress;
            IPAddress remoteIP;

            // Only check if forwarded by a local network reverse proxy
            if (IPAddress.TryParse(remoteAddress, out remoteIP) && remoteIP.IsLocalAddress())
            {
                var realIPHeader = context.Request.Headers["X-Real-IP"];
                if (realIPHeader.Any())
                {
                    return realIPHeader.First().ToString();
                }

                var forwardedForHeader = context.Request.Headers["X-Forwarded-For"];
                if (forwardedForHeader.Any())
                {
                    // Get the first address that was forwarded by a local IP to prevent remote clients faking another proxy
                    foreach (var forwardedForAddress in forwardedForHeader.SelectMany(v => v.Split(',')).Select(v => v.Trim()).Reverse())
                    {
                        if (!IPAddress.TryParse(forwardedForAddress, out remoteIP))
                        {
                            return remoteAddress;
                        }

                        if (!remoteIP.IsLocalAddress())
                        {
                            return forwardedForAddress;
                        }

                        remoteAddress = forwardedForAddress;
                    }
                }
            }

            return remoteAddress;
        }
    }
}
