using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Update
{
    public class InstallUpdateService : IExecute<ApplicationUpdateCommand>
    {
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly Logger _logger;
        private readonly IAppFolderInfo _appFolderInfo;

        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IHttpClient _httpClient;
        private readonly IArchiveService _archiveService;
        private readonly IProcessProvider _processProvider;
        private readonly IVerifyUpdates _updateVerifier;
        private readonly IStartupContext _startupContext;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IBackupService _backupService;


        public InstallUpdateService(ICheckUpdateService checkUpdateService,
                                    IAppFolderInfo appFolderInfo,
                                    IDiskProvider diskProvider,
                                    IDiskTransferService diskTransferService,
                                    IHttpClient httpClient,
                                    IArchiveService archiveService,
                                    IProcessProvider processProvider,
                                    IVerifyUpdates updateVerifier,
                                    IStartupContext startupContext,
                                    IConfigFileProvider configFileProvider,
                                    IRuntimeInfo runtimeInfo,
                                    IBackupService backupService,
                                    Logger logger)
        {
            if (configFileProvider == null)
            {
                throw new ArgumentNullException(nameof(configFileProvider));
            }
            _checkUpdateService = checkUpdateService;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _httpClient = httpClient;
            _archiveService = archiveService;
            _processProvider = processProvider;
            _updateVerifier = updateVerifier;
            _startupContext = startupContext;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _backupService = backupService;
            _logger = logger;
        }

        private void InstallUpdate(UpdatePackage updatePackage)
        {
            EnsureAppDataSafety();

            if (OsInfo.IsWindows || _configFileProvider.UpdateMechanism != UpdateMechanism.Script)
            {
                var startupFolder = _appFolderInfo.StartUpFolder;
                var uiFolder = Path.Combine(startupFolder, "UI");

                if (!_diskProvider.FolderWritable(startupFolder))
                {
                    throw new UpdateFolderNotWritableException("Cannot install update because startup folder '{0}' is not writable by the user '{1}'.", startupFolder, Environment.UserName);
                }

                if (!_diskProvider.FolderWritable(uiFolder))
                {
                    throw new UpdateFolderNotWritableException("Cannot install update because UI folder '{0}' is not writable by the user '{1}'.", uiFolder, Environment.UserName);
                }
            }

            var updateSandboxFolder = _appFolderInfo.GetUpdateSandboxFolder();

            var packageDestination = Path.Combine(updateSandboxFolder, updatePackage.FileName);

            if (_diskProvider.FolderExists(updateSandboxFolder))
            {
                _logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(updateSandboxFolder, true);
            }

            _logger.ProgressInfo("Downloading update {0}", updatePackage.Version);
            _logger.Debug("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            _httpClient.DownloadFile(updatePackage.Url, packageDestination);

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

            EnsureValidBranch(updatePackage);

            _backupService.Backup(BackupType.Update);

            if (OsInfo.IsNotWindows && _configFileProvider.UpdateMechanism == UpdateMechanism.Script)
            {
                InstallUpdateWithScript(updateSandboxFolder);
                return;
            }

            _logger.Info("Preparing client");
            _diskTransferService.TransferFolder(_appFolderInfo.GetUpdateClientFolder(), updateSandboxFolder, TransferMode.Move, false);

            _logger.Info("Starting update client {0}", _appFolderInfo.GetUpdateClientExePath());
            _logger.ProgressInfo("Sonarr will restart shortly.");

            _processProvider.Start(_appFolderInfo.GetUpdateClientExePath(), GetUpdaterArgs(updateSandboxFolder));
        }

        private void EnsureValidBranch(UpdatePackage package)
        {
            var currentBranch = _configFileProvider.Branch;
            if (package.Branch != currentBranch)
            {
                try
                {
                    _logger.Info("Branch [{0}] is being redirected to [{1}]]", currentBranch, package.Branch);
                    var config = new Dictionary<string, object>();
                    config["Branch"] = package.Branch;
                    _configFileProvider.SaveConfigDictionary(config);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't change the branch from [{0}] to [{1}].", currentBranch, package.Branch);
                }
            }
        }

        private void InstallUpdateWithScript(string updateSandboxFolder)
        {
            var scriptPath = _configFileProvider.UpdateScriptPath;

            if (scriptPath.IsNullOrWhiteSpace())
            {
                throw new UpdateFailedException("Update Script has not been defined");
            }

            if (!_diskProvider.FileExists(scriptPath, StringComparison.Ordinal))
            {
                throw new UpdateFailedException("Update Script: '{0}' does not exist", scriptPath);
            }

            _logger.Info("Removing Sonarr.Update");
            _diskProvider.DeleteFolder(_appFolderInfo.GetUpdateClientFolder(), true);

            _logger.ProgressInfo("Starting update script: {0}", _configFileProvider.UpdateScriptPath);
            _processProvider.Start(scriptPath, GetUpdaterArgs(updateSandboxFolder));
        }

        private string GetUpdaterArgs(string updateSandboxFolder)
        {
            var processId = _processProvider.GetCurrentProcess().Id.ToString();
            var executingApplication = _runtimeInfo.ExecutingApplication;

            return string.Join(" ", processId, updateSandboxFolder.TrimEnd(Path.DirectorySeparatorChar).WrapInQuotes(), executingApplication.WrapInQuotes(), _startupContext.PreservedArguments);
        }

        private void EnsureAppDataSafety()
        {
            if (_appFolderInfo.StartUpFolder.IsParentPath(_appFolderInfo.AppDataFolder) ||
                _appFolderInfo.StartUpFolder.PathEquals(_appFolderInfo.AppDataFolder))
            {
                throw new UpdateFailedException("Your Sonarr configuration '{0}' is being stored in application folder '{1}' which will cause data lost during the upgrade. Please remove any symlinks or redirects before trying again.", _appFolderInfo.AppDataFolder, _appFolderInfo.StartUpFolder);
            }
        }

        public void Execute(ApplicationUpdateCommand message)
        {
            _logger.ProgressDebug("Checking for updates");

            var latestAvailable = _checkUpdateService.AvailableUpdate();

            if (latestAvailable == null)
            {
                _logger.ProgressDebug("No update available");
                return;
            }

            if (OsInfo.IsNotWindows && !_configFileProvider.UpdateAutomatically && message.Trigger != CommandTrigger.Manual)
            {
                _logger.ProgressDebug("Auto-update not enabled, not installing available update");
                return;
            }

            try
            {
                InstallUpdate(latestAvailable);
                _logger.ProgressDebug("Restarting Sonarr to apply updates");
            }
            catch (UpdateFolderNotWritableException ex)
            {
                _logger.Error(ex, "Update process failed");
                throw new CommandFailedException("Startup folder not writable by user '{0}'", ex, Environment.UserName);
            }
            catch (UpdateVerificationFailedException ex)
            {
                _logger.Error(ex, "Update process failed");
                throw new CommandFailedException("Downloaded update package is corrupt", ex);
            }
            catch (UpdateFailedException ex)
            {
                _logger.Error(ex, "Update process failed");
                throw new CommandFailedException(ex);
            }
        }
    }
}
