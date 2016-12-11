using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IBackupAndRestore
    {
        void Backup(string source);
        void Restore(string target);
    }

    public class BackupAndRestore : IBackupAndRestore
    {
        private readonly IDiskTransferService _diskTransferService;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly Logger _logger;

        public BackupAndRestore(IDiskTransferService diskTransferService, IAppFolderInfo appFolderInfo, Logger logger)
        {
            _diskTransferService = diskTransferService;
            _appFolderInfo = appFolderInfo;
            _logger = logger;
        }

        public void Backup(string source)
        {
            _logger.Info("Creating backup of existing installation");
            _diskTransferService.MirrorFolder(source, _appFolderInfo.GetUpdateBackUpFolder());
        }

        public void Restore(string target)
        {
            _logger.Info("Attempting to rollback upgrade");
            var count = _diskTransferService.MirrorFolder(_appFolderInfo.GetUpdateBackUpFolder(), target);
            _logger.Info("Rolled back {0} files", count);
        }
    }
}
