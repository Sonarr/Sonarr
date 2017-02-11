using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public class BackupFileMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public BackupFileMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace("/backup/", "").Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(_appFolderInfo.GetBackupFolder(), path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/backup/") && resourceUrl.ContainsIgnoreCase("nzbdrone_backup_")  && resourceUrl.EndsWith(".zip");
        }
    }
}