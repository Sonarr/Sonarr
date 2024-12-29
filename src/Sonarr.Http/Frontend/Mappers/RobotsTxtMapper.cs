using System.IO;
using NLog;
using Workarr.Configuration;
using Workarr.Disk;
using Workarr.EnvironmentInfo;

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

        public override string Map(string resourceUrl)
        {
            var path = Path.Combine("Content", "robots.txt");

            return Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.Equals("/robots.txt");
        }
    }
}
