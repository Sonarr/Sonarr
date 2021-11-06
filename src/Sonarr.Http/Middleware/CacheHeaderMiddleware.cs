using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Middleware
{
    public class CacheHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICacheableSpecification _cacheableSpecification;

        public CacheHeaderMiddleware(RequestDelegate next, ICacheableSpecification cacheableSpecification)
        {
            _next = next;
            _cacheableSpecification = cacheableSpecification;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method != "OPTIONS")
            {
                if (_cacheableSpecification.IsCacheable(context.Request))
                {
                    context.Response.Headers.EnableCache();
                }
                else
                {
                    context.Response.Headers.DisableCache();
                }
            }

            await _next(context);
        }
    }
}
