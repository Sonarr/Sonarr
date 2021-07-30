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
            return request.Path.CleanRequestPath().StartsWith("/api/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFeedRequest(this Request request)
        {
            return request.Path.CleanRequestPath().StartsWith("/feed/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsPingRequest(this Request request)
        {
            return request.Path.CleanRequestPath().StartsWith("/ping", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSignalRRequest(this Request request)
        {
            return request.Path.CleanRequestPath().StartsWith("/signalr/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsLocalRequest(this Request request)
        {
            return (request.UserHostAddress.Equals("localhost") ||
                    request.UserHostAddress.Equals("127.0.0.1") ||
                    request.UserHostAddress.Equals("::1"));
        }

        public static bool IsLoginRequest(this Request request)
        {
            return request.Path.CleanRequestPath().Equals("/login", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsContentRequest(this Request request)
        {
            return request.Path.CleanRequestPath().StartsWith("/Content/", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsBundledJsRequest(this Request request)
        {
            return !request.Path.CleanRequestPath().EqualsIgnoreCase("/initialize.js") && request.Path.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsFavIconRequest(this Request request)
        {
            return request.Path.CleanRequestPath().EqualsIgnoreCase("/favicon.ico");
        }

        public static bool IsSharedContentRequest(this Request request)
        {
            return request.Path.CleanRequestPath().StartsWith("/MediaCover/", StringComparison.InvariantCultureIgnoreCase) ||
                   request.Path.CleanRequestPath().StartsWith("/Content/Images/", StringComparison.InvariantCultureIgnoreCase);
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

            IPAddress.TryParse(remoteAddress, out IPAddress remoteIP);

            if (remoteIP == null)
            {
                return remoteAddress;
            }

            if (remoteIP.IsIPv4MappedToIPv6)
            {
                remoteIP = remoteIP.MapToIPv4();
            }

            remoteAddress = remoteIP.ToString();

            // Only check if forwarded by a local network reverse proxy
            if (remoteIP.IsLocalAddress())
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

        private static string CleanRequestPath(this string path)
        {
            // When running under mono the path is not stripped of extraneous leading slashes which can break our IXRequest
            // path detection, this will remove all leading slashes and replace them with a single slash.

            return $"/{path.TrimStart('/')}";
        }
    }
}
