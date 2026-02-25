using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Backup;

namespace Sonarr.Http.Frontend.Mappers
{
    public class BackupFileMapper : StaticResourceMapperBase
    {
        private readonly IBackupService _backupService;

        public BackupFileMapper(IBackupService backupService, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _backupService = backupService;
        }

        protected override string FolderPath => _backupService.GetBackupFolder();

        protected override string MapPath(string resourceUrl)
        {
            var path = resourceUrl.Replace("/backup/", "").Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(FolderPath, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/backup/") && BackupService.BackupFileRegex.IsMatch(resourceUrl);
        }
    }
}
