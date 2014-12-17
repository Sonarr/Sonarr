using System;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

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

            try
            {
                _diskProvider.CopyFile(_appFolderInfo.GetConfigPath(), _appFolderInfo.GetUpdateBackupConfigFile(), true);
                _diskProvider.CopyFile(_appFolderInfo.GetNzbDroneDatabase(), _appFolderInfo.GetUpdateBackupDatabase(),
                    true);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't create a data backup", e);
            }
        }
    }
}
