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
    }
}
