using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sonarr.Http.Middleware
{
    public class BufferingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<string> _urls;

        public BufferingMiddleware(RequestDelegate next, List<string> urls)
        {
            _next = next;
            _urls = urls;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_urls.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            {
                context.Request.EnableBuffering();
            }

            await _next(context);
        }
    }
}
