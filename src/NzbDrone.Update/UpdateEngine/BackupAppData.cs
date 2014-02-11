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
            var backupFolderAppData = _appFolderInfo.GetUpdateBackUpAppDataFolder();

            _diskProvider.CreateFolder(backupFolderAppData);
            _diskProvider.CopyFile(_appFolderInfo.GetConfigPath(), _appFolderInfo.GetUpdateBackupConfigFile(), true);
            _diskProvider.CopyFile(_appFolderInfo.GetNzbDroneDatabase(), _appFolderInfo.GetUpdateBackupDatabase(), true);
        }
    }
}
