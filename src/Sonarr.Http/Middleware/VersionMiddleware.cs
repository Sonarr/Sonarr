using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Middleware
{
    public class VersionMiddleware
    {
        private const string VERSIONHEADER = "X-Application-Version";

        private static readonly string Version = BuildInfo.Version.ToString();

        private readonly RequestDelegate _next;

        public VersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.IsApiRequest())
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[VERSIONHEADER] = Version;

                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }
    }
}
