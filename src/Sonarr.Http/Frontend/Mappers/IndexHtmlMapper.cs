using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using StackExchange.Profiling;

namespace Sonarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, configFileProvider, cacheBreakProviderFactory, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        protected override string FolderPath => Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder);
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
                   !resourceUrl.StartsWith("/login");
        }

        protected override string GetHtmlText(HttpContext context)
        {
            var html = base.GetHtmlText(context);
            var theme = UiTheme.Normalize(_configFileProvider.Theme);

            html = html.Replace("_THEME_", theme)
                       .Replace("__THEME__", theme);

            var profilerHtml = _configFileProvider.ProfilerEnabled
                ? MiniProfiler.Current?.RenderIncludes(context)?.Value ?? ""
                : "";

            html = html.Replace("__MINI_PROFILER__", profilerHtml);

            return html;
        }
    }
}
