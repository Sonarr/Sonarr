using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class RobotsTxtMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public RobotsTxtMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = Path.Combine("Content", "robots.txt");

            return Path.Combine(_appFolderInfo.StartUpFolder, "UI", path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.Equals("/robots.txt");
        }
    }
}
