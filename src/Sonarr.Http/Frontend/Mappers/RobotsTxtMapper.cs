using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class RobotsTxtMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public RobotsTxtMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        protected override string FolderPath => Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder);

        protected override string MapPath(string resourceUrl)
        {
            var path = Path.Combine("Content", "robots.txt");

            return Path.Combine(FolderPath, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.Equals("/robots.txt");
        }
    }
}
