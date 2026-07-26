using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using StackExchange.Profiling;

namespace Sonarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IViteDevServer _viteDevServer;
        private readonly Logger _logger;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               IViteDevServer viteDevServer,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, configFileProvider, cacheBreakProviderFactory, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
            _viteDevServer = viteDevServer;
            _logger = logger;
        }

        protected override string FolderPath => _appFolderInfo.GetUiFolder();
        protected override string HtmlPath => Path.Combine(FolderPath, "index.html");

        protected override string MapPath(string resourceUrl)
        {
            return HtmlPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            return !resourceUrl.StartsWith("/content") &&
                   !resourceUrl.StartsWith("/mediacover") &&
                   !resourceUrl.Contains('.') &&
                   !resourceUrl.StartsWith("/login") &&
                   !resourceUrl.StartsWith("/logout");
        }

        protected override string ReadHtml()
        {
            if (_viteDevServer.IsEnabled)
            {
                try
                {
                    return _viteDevServer.GetIndexHtmlAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    _logger.Debug("Vite dev server is unavailable, serving built UI: {0}", ex.Message);
                }
            }

            return base.ReadHtml();
        }

        protected override string GetHtmlText(HttpContext context)
        {
            var html = base.GetHtmlText(context);
            var theme = _configFileProvider.Theme;

            html = html.Replace("_THEME_", theme);

            if (_configFileProvider.ProfilerEnabled)
            {
                var includes = MiniProfiler.Current?.RenderIncludes(context);

                if (includes == null || includes.Value.IsNullOrWhiteSpace())
                {
                    html = html.Replace("__MINI_PROFILER__", "");
                }
                else
                {
                    html = html.Replace("__MINI_PROFILER__", includes.Value);
                }
            }
            else
            {
                html = html.Replace("__MINI_PROFILER__", "");
            }

            return html;
        }
    }
}
