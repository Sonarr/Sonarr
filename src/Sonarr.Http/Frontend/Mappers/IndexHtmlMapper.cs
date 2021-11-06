using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase
    {
        private readonly IConfigFileProvider _configFileProvider;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileProvider = configFileProvider;

            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, "index.html");
            UrlBase = configFileProvider.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return HtmlPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            return !resourceUrl.StartsWith("/content") &&
                   !resourceUrl.StartsWith("/mediacover") &&
                   !resourceUrl.Contains(".") &&
                   !resourceUrl.StartsWith("/login");
        }
    }
}
