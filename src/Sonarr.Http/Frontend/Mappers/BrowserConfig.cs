using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class BrowserConfig : UrlBaseReplacementResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public BrowserConfig(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, configFileProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        protected override string FolderPath => Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder);
        protected override string FilePath => Path.Combine(FolderPath, "Content", "browserconfig.xml");

        protected override string MapPath(string resourceUrl)
        {
            return FilePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content/browserconfig");
        }
    }
}
