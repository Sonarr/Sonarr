using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Extensions;
using Sonarr.Http.Frontend;

namespace Sonarr.Http.Middleware
{
    public class ViteDevMiddleware
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(ViteDevMiddleware));

        private readonly RequestDelegate _next;
        private readonly IViteDevServer _viteDevServer;
        private readonly string _urlBase;

        public ViteDevMiddleware(RequestDelegate next, IViteDevServer viteDevServer, string urlBase)
        {
            _next = next;
            _viteDevServer = viteDevServer;
            _urlBase = urlBase;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (_urlBase.IsNotNullOrWhiteSpace() && path.StartsWith(_urlBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(_urlBase.Length);
            }

            if (!_viteDevServer.HandlesPath(path))
            {
                await _next(context);

                return;
            }

            HttpResponseMessage response;

            try
            {
                response = await _viteDevServer.GetAsync(path, context.Request.QueryString.Value);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Unable to reach the Vite dev server for {0}. Is it running and is SONARR_VITE_DEV_SERVER pointing at it?", path);
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;

                return;
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex, "Timed out waiting for the Vite dev server for {0}. Is it running and is SONARR_VITE_DEV_SERVER pointing at it?", path);
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;

                return;
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();

                    _logger.Warn("Vite dev server returned {0} for {1}: {2}", response.StatusCode, path, body);
                }

                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                await response.Content.CopyToAsync(context.Response.Body);
            }
        }
    }
}
