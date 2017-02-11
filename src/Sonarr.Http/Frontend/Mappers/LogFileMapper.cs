using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public class UpdateLogFileMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public UpdateLogFileMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = Path.GetFileName(path);

            return Path.Combine(_appFolderInfo.GetUpdateLogFolder(), path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/updatelogfile/") && resourceUrl.EndsWith(".txt");
        }
    }
}