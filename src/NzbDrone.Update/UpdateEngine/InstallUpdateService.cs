using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IInstallUpdateService
    {
        void Start(string installationFolder);
    }

    public class InstallUpdateService : IInstallUpdateService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDetectApplicationType _detectApplicationType;
        private readonly ITerminateNzbDrone _terminateNzbDrone;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IBackupAndRestore _backupAndRestore;
        private readonly IBackupAppData _backupAppData;
        private readonly IStartNzbDrone _startNzbDrone;
        private readonly Logger _logger;

        public InstallUpdateService(IDiskProvider diskProvider,
                                    IDetectApplicationType detectApplicationType,
                                    ITerminateNzbDrone terminateNzbDrone,
                                    IAppFolderInfo appFolderInfo,
                                    IBackupAndRestore backupAndRestore,
                                    IBackupAppData backupAppData,
                                    IStartNzbDrone startNzbDrone,
                                    Logger logger)
        {
            _diskProvider = diskProvider;
            _detectApplicationType = detectApplicationType;
            _terminateNzbDrone = terminateNzbDrone;
            _appFolderInfo = appFolderInfo;
            _backupAndRestore = backupAndRestore;
            _backupAppData = backupAppData;
            _startNzbDrone = startNzbDrone;
            _logger = logger;
        }

        private void Verify(string targetFolder)
        {
            _logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(targetFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist " + targetFolder);

            _logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_appFolderInfo.GetUpdatePackageFolder()))
                throw new DirectoryNotFoundException("Update folder doesn't exist " + _appFolderInfo.GetUpdatePackageFolder());
        }

        public void Start(string installationFolder)
        {
            Verify(installationFolder);

            var appType = _detectApplicationType.GetAppType();

            try
            {
                _terminateNzbDrone.Terminate();

                _backupAndRestore.Backup(installationFolder);
                _backupAppData.Backup();

                _logger.Info("Moving update package to target");

                try
                {
                    _diskProvider.EmptyFolder(installationFolder);
                    _diskProvider.CopyFolder(_appFolderInfo.GetUpdatePackageFolder(), installationFolder);
                }
                catch (Exception e)
                {
                    _backupAndRestore.Restore(installationFolder);
                    _logger.FatalException("Failed to copy upgrade package to target folder.", e);
                }

            }
            finally
            {
                _startNzbDrone.Start(appType, installationFolder);
            }

        }
    }
}
