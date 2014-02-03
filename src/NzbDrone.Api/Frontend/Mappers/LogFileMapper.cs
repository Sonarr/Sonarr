using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class LogFileMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public LogFileMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        protected override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = Path.GetFileName(path);

            return Path.Combine(_appFolderInfo.GetLogFolder(), path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/log/") && resourceUrl.EndsWith(".txt");
        }
    }
}