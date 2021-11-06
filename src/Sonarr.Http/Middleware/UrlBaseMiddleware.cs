using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Middleware
{
    public class UrlBaseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _urlBase;

        public UrlBaseMiddleware(RequestDelegate next, string urlBase)
        {
            _next = next;
            _urlBase = urlBase;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_urlBase.IsNotNullOrWhiteSpace() && context.Request.PathBase.Value.IsNullOrWhiteSpace())
            {
                context.Response.Redirect($"{_urlBase}{context.Request.Path}{context.Request.QueryString}");
                return;
            }

            await _next(context);
        }
    }
}
