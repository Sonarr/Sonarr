using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Sonarr.Http.Frontend.Mappers
{
    public class ViteDevMapper : IMapHttpRequestsToDisk
    {
        private readonly IViteDevServer _viteDevServer;
        private readonly Logger _logger;

        public ViteDevMapper(IViteDevServer viteDevServer, Logger logger)
        {
            _viteDevServer = viteDevServer;
            _logger = logger;
        }

        public string Map(string resourceUrl)
        {
            return null;
        }

        public bool CanHandle(string resourceUrl)
        {
            return _viteDevServer.HandlesPath(resourceUrl);
        }

        public async Task<IActionResult> GetResponse(HttpContext context, string resourceUrl)
        {
            HttpResponseMessage response;

            try
            {
                response = await _viteDevServer.GetAsync(resourceUrl, context.Request.QueryString.Value);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Unable to reach the Vite dev server for {0}. Is it running and is SONARR_VITE_DEV_SERVER pointing at it?", resourceUrl);

                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error(ex, "Timed out waiting for the Vite dev server for {0}. Is it running and is SONARR_VITE_DEV_SERVER pointing at it?", resourceUrl);

                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                _logger.Warn("Vite dev server returned {0} for {1}: {2}", response.StatusCode, resourceUrl, body);

                return null;
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            return new FileStreamResult(await response.Content.ReadAsStreamAsync(), contentType);
        }
    }
}
