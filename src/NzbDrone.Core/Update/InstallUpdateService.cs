using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Update
{
    public interface IInstallUpdates
    {
        void InstallUpdate(UpdatePackage updatePackage);
    }

    public class InstallUpdateService : IInstallUpdates, IExecute<ApplicationUpdateCommand>, IExecute<InstallUpdateCommand>
    {
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly Logger _logger;
        private readonly IAppFolderInfo _appFolderInfo;

        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly IArchiveService _archiveService;
        private readonly IProcessProvider _processProvider;
        private readonly IVerifyUpdates _updateVerifier;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IBackupService _backupService;


        public InstallUpdateService(ICheckUpdateService checkUpdateService, IAppFolderInfo appFolderInfo,
                                    IDiskProvider diskProvider, IHttpProvider httpProvider,
                                    IArchiveService archiveService, IProcessProvider processProvider,
                                    IVerifyUpdates updateVerifier,
                                    IConfigFileProvider configFileProvider,
                                    IRuntimeInfo runtimeInfo,
                                    IBackupService backupService,
                                    Logger logger)
        {
            if (configFileProvider == null)
            {
                throw new ArgumentNullException("configFileProvider");
            }
            _checkUpdateService = checkUpdateService;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _archiveService = archiveService;
            _processProvider = processProvider;
            _updateVerifier = updateVerifier;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _backupService = backupService;
            _logger = logger;
        }

        public void InstallUpdate(UpdatePackage updatePackage)
        {
            try
            {
                EnsureAppDataSafety();

                var updateSandboxFolder = _appFolderInfo.GetUpdateSandboxFolder();

                var packageDestination = Path.Combine(updateSandboxFolder, updatePackage.FileName);

                if (_diskProvider.FolderExists(updateSandboxFolder))
                {
                    _logger.Info("Deleting old update files");
                    _diskProvider.DeleteFolder(updateSandboxFolder, true);
                }

                _logger.ProgressInfo("Downloading update {0}", updatePackage.Version);
                _logger.Debug("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
                _httpProvider.DownloadFile(updatePackage.Url, packageDestination);

                _logger.ProgressInfo("Verifying update package");

                if (!_updateVerifier.Verify(updatePackage, packageDestination))
                {
                    _logger.Error("Update package is invalid");
                    throw new UpdateVerificationFailedException("Update file '{0}' is invalid", packageDestination);
                }

                _logger.Info("Update package verified successfully");

                _logger.ProgressInfo("Extracting Update package");
                _archiveService.Extract(packageDestination, updateSandboxFolder);
                _logger.Info("Update package extracted successfully");

                _backupService.Backup(BackupType.Update);

                if (OsInfo.IsMono && _configFileProvider.UpdateMechanism == UpdateMechanism.Script)
                {
                    InstallUpdateWithScript(updateSandboxFolder);
                    return;
                }

                _logger.Info("Preparing client");
                _diskProvider.MoveFolder(_appFolderInfo.GetUpdateClientFolder(),
                                            updateSandboxFolder);

                _logger.Info("Starting update client {0}", _appFolderInfo.GetUpdateClientExePath());
                _logger.ProgressInfo("NzbDrone will restart shortly.");

                _processProvider.Start(_appFolderInfo.GetUpdateClientExePath(), GetUpdaterArgs(updateSandboxFolder));
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Update process failed", ex);
            }
        }

        private void InstallUpdateWithScript(String updateSandboxFolder)
        {
            var scriptPath = _configFileProvider.UpdateScriptPath;

            if (scriptPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Update Script has not been defined");
            }

            if (!_diskProvider.FileExists(scriptPath, true))
            {
                var message = String.Format("Update Script: '{0}' does not exist", scriptPath);
                throw new FileNotFoundException(message, scriptPath);
            }

            _logger.Info("Removing NzbDrone.Update");
            _diskProvider.DeleteFolder(_appFolderInfo.GetUpdateClientFolder(), true);

            _logger.ProgressInfo("Starting update script: {0}", _configFileProvider.UpdateScriptPath);
            _processProvider.Start(scriptPath, GetUpdaterArgs(updateSandboxFolder));
        }

        private string GetUpdaterArgs(string updateSandboxFolder)
        {
            var processId = _processProvider.GetCurrentProcess().Id.ToString();
            var executingApplication = _runtimeInfo.ExecutingApplication;

            return String.Join(" ", processId, updateSandboxFolder.TrimEnd(Path.DirectorySeparatorChar).WrapInQuotes(), executingApplication.WrapInQuotes());
        }

        private void EnsureAppDataSafety()
        {
            if (_appFolderInfo.StartUpFolder.IsParentPath(_appFolderInfo.AppDataFolder) ||
                _appFolderInfo.StartUpFolder.PathEquals(_appFolderInfo.AppDataFolder))
            {
                throw new NotSupportedException("Update will cause AppData to be deleted, correct you configuration before proceeding");
            }
        }

        public void Execute(ApplicationUpdateCommand message)
        {
            _logger.ProgressDebug("Checking for updates");
            var latestAvailable = _checkUpdateService.AvailableUpdate();

            if (latestAvailable != null)
            {
                InstallUpdate(latestAvailable);
            }
        }

        public void Execute(InstallUpdateCommand message)
        {
            InstallUpdate(message.UpdatePackage);
        }
    }
}
