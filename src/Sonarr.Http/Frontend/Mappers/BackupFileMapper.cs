using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Backup;

namespace Sonarr.Http.Frontend.Mappers
{
    public class BackupFileMapper : StaticResourceMapperBase
    {
        private readonly IBackupService _backupService;
        private readonly IAppFolderInfo _appFolderInfo;

        public BackupFileMapper(IBackupService backupService, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _backupService = backupService;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace("/backup/", "").Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(_backupService.GetBackupFolder(), path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/backup/") && BackupService.BackupFileRegex.IsMatch(resourceUrl);
        }
    }
}
