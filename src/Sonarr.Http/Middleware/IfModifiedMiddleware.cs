using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Sonarr.Http.Extensions;

namespace Sonarr.Http.Middleware
{
    public class IfModifiedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICacheableSpecification _cacheableSpecification;
        private readonly IContentTypeProvider _mimeTypeProvider;

        public IfModifiedMiddleware(RequestDelegate next, ICacheableSpecification cacheableSpecification)
        {
            _next = next;
            _cacheableSpecification = cacheableSpecification;

            _mimeTypeProvider = new FileExtensionContentTypeProvider();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_cacheableSpecification.IsCacheable(context.Request) && context.Request.Headers["IfModifiedSince"].Any())
            {
                context.Response.StatusCode = 304;
                context.Response.Headers.EnableCache();

                if (!_mimeTypeProvider.TryGetContentType(context.Request.Path.ToString(), out var mimeType))
                {
                    mimeType = "application/octet-stream";
                }

                context.Response.ContentType = mimeType;

                return;
            }

            await _next(context);
        }
    }
}
