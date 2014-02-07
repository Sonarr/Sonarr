using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IBackupAppData
    {
        void Backup();
    }

    public class BackupAppData : IBackupAppData
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public BackupAppData(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void Backup()
        {
            _logger.Info("Backing up appdata (database/config)");
            var appDataPath = _appFolderInfo.GetAppDataPath();
            var backupFolderAppData = _appFolderInfo.GetUpdateBackUpAppDataFolder();
            var binFolder = Path.Combine(backupFolderAppData, "bin");

            _diskProvider.CreateFolder(backupFolderAppData);
            _diskProvider.CopyFolder(appDataPath, backupFolderAppData);

            if (_diskProvider.FolderExists(binFolder))
            {
                _logger.Info("Deleting bin folder from appdata");
                _diskProvider.DeleteFolder(binFolder, true);
            }
        }
    }
}
