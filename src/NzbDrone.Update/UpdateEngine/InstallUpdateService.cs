using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IInstallUpdateService
    {
        void Start(string installationFolder, int processId);
    }

    public class InstallUpdateService : IInstallUpdateService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDetectApplicationType _detectApplicationType;
        private readonly IDetectExistingVersion _detectExistingVersion;
        private readonly ITerminateNzbDrone _terminateNzbDrone;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IBackupAndRestore _backupAndRestore;
        private readonly IBackupAppData _backupAppData;
        private readonly IStartNzbDrone _startNzbDrone;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public InstallUpdateService(IDiskProvider diskProvider,
                                    IDiskTransferService diskTransferService,
                                    IDetectApplicationType detectApplicationType,
                                    IDetectExistingVersion detectExistingVersion,
                                    ITerminateNzbDrone terminateNzbDrone,
                                    IAppFolderInfo appFolderInfo,
                                    IBackupAndRestore backupAndRestore,
                                    IBackupAppData backupAppData,
                                    IStartNzbDrone startNzbDrone,
                                    IProcessProvider processProvider,
                                    Logger logger)
        {
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _detectApplicationType = detectApplicationType;
            _detectExistingVersion = detectExistingVersion;
            _terminateNzbDrone = terminateNzbDrone;
            _appFolderInfo = appFolderInfo;
            _backupAndRestore = backupAndRestore;
            _backupAppData = backupAppData;
            _startNzbDrone = startNzbDrone;
            _processProvider = processProvider;
            _logger = logger;
        }

        private void Verify(string targetFolder, int processId)
        {
            _logger.Info("Verifying requirements before update...");

            if (string.IsNullOrWhiteSpace(targetFolder))
            {
                throw new ArgumentException("Target folder can not be null or empty");
            }

            if (!_diskProvider.FolderExists(targetFolder))
            {
                throw new DirectoryNotFoundException("Target folder doesn't exist " + targetFolder);
            }

            if (processId < 1)
            {
                throw new ArgumentException("Invalid process ID: " + processId);
            }

            if (!_processProvider.Exists(processId))
            {
                throw new ArgumentException("Process with ID doesn't exist " + processId);
            }

            _logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_appFolderInfo.GetUpdatePackageFolder()))
            {
                throw new DirectoryNotFoundException("Update folder doesn't exist " + _appFolderInfo.GetUpdatePackageFolder());
            }
        }

        public void Start(string installationFolder, int processId)
        {
            _logger.Info("Installation Folder: {0}", installationFolder);
            _logger.Info("Updating Sonarr from version {0} to version {1}", _detectExistingVersion.GetExistingVersion(installationFolder), BuildInfo.Version);

            Verify(installationFolder, processId);

            if (installationFolder.EndsWith(@"\bin\Sonarr") || installationFolder.EndsWith(@"/bin/Sonarr"))
            {
                installationFolder = installationFolder.GetParentPath();
                _logger.Info("Fixed Installation Folder: {0}", installationFolder);
            }

            var appType = _detectApplicationType.GetAppType();

            _processProvider.FindProcessByName(ProcessProvider.SONARR_CONSOLE_PROCESS_NAME);
            _processProvider.FindProcessByName(ProcessProvider.SONARR_PROCESS_NAME);

            if (OsInfo.IsWindows)
            {
                _terminateNzbDrone.Terminate(processId);
            }

            try
            {
                _backupAndRestore.Backup(installationFolder);
                _backupAppData.Backup();

                if (OsInfo.IsWindows)
                {
                    if (_processProvider.Exists(ProcessProvider.SONARR_CONSOLE_PROCESS_NAME) || _processProvider.Exists(ProcessProvider.SONARR_PROCESS_NAME))
                    {
                        _logger.Error("Sonarr was restarted prematurely by external process.");
                        return;
                    }
                }

                try
                {
                    _logger.Info("Copying new files to target folder");
                    _diskTransferService.MirrorFolder(_appFolderInfo.GetUpdatePackageFolder(), installationFolder);

                    // Set executable flag on app and ffprobe
                    if (OsInfo.IsOsx || (OsInfo.IsLinux && PlatformInfo.IsNetCore))
                    {
                        _diskProvider.SetFilePermissions(Path.Combine(installationFolder, "Sonarr"), "755", null);
                        _diskProvider.SetFilePermissions(Path.Combine(installationFolder, "ffprobe"), "755", null);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to copy upgrade package to target folder.");
                    _backupAndRestore.Restore(installationFolder);
                    throw;
                }
            }
            finally
            {
                if (OsInfo.IsWindows)
                {
                    _startNzbDrone.Start(appType, installationFolder);
                }
                else
                {
                    _terminateNzbDrone.Terminate(processId);

                    _logger.Info("Waiting for external auto-restart.");
                    var theDakoLimit = 10;
                    for (int i = 0; i < theDakoLimit; i++)
                    {
                        System.Threading.Thread.Sleep(1000);

                        if (_processProvider.Exists(ProcessProvider.SONARR_PROCESS_NAME))
                        {
                            _logger.Info("Sonarr was restarted by external process.");
                            break;
                        }
                    }

                    if (!_processProvider.Exists(ProcessProvider.SONARR_PROCESS_NAME))
                    {
                        _startNzbDrone.Start(appType, installationFolder);
                    }
                }
            }
        }
    }
}
