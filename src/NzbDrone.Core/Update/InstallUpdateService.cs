using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Update
{
    public class InstallUpdateService : IExecute<ApplicationUpdateCommand>, IExecute<ApplicationUpdateCheckCommand>, IHandle<ApplicationStartingEvent>
    {
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly Logger _logger;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IHttpClient _httpClient;
        private readonly IArchiveService _archiveService;
        private readonly IProcessProvider _processProvider;
        private readonly IVerifyUpdates _updateVerifier;
        private readonly IStartupContext _startupContext;
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IBackupService _backupService;
        private readonly IOsInfo _osInfo;

        public InstallUpdateService(ICheckUpdateService checkUpdateService,
                                    IAppFolderInfo appFolderInfo,
                                    IManageCommandQueue commandQueueManager,
                                    IDiskProvider diskProvider,
                                    IDiskTransferService diskTransferService,
                                    IHttpClient httpClient,
                                    IArchiveService archiveService,
                                    IProcessProvider processProvider,
                                    IVerifyUpdates updateVerifier,
                                    IStartupContext startupContext,
                                    IDeploymentInfoProvider deploymentInfoProvider,
                                    IConfigFileProvider configFileProvider,
                                    IRuntimeInfo runtimeInfo,
                                    IBackupService backupService,
                                    IOsInfo osInfo,
                                    Logger logger)
        {
            if (configFileProvider == null)
            {
                throw new ArgumentNullException(nameof(configFileProvider));
            }

            _checkUpdateService = checkUpdateService;
            _appFolderInfo = appFolderInfo;
            _commandQueueManager = commandQueueManager;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _httpClient = httpClient;
            _archiveService = archiveService;
            _processProvider = processProvider;
            _updateVerifier = updateVerifier;
            _startupContext = startupContext;
            _deploymentInfoProvider = deploymentInfoProvider;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _backupService = backupService;
            _osInfo = osInfo;
            _logger = logger;
        }

        private bool InstallUpdate(UpdatePackage updatePackage)
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

            if (_appFolderInfo.StartUpFolder.EndsWith("_output"))
            {
                _logger.ProgressDebug("Running in developer environment, not updating.");
                return false;
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
                return true;
            }

            _logger.Info("Preparing client");
            _diskTransferService.TransferFolder(_appFolderInfo.GetUpdateClientFolder(), updateSandboxFolder, TransferMode.Move);

            var updateClientExePath = _appFolderInfo.GetUpdateClientExePath(updatePackage.Runtime);

            if (!_diskProvider.FileExists(updateClientExePath))
            {
                _logger.Warn("Update client {0} does not exist, aborting update.", updateClientExePath);
                return false;
            }

            // Set executable flag on update app
            if (OsInfo.IsOsx || (OsInfo.IsLinux && PlatformInfo.IsNetCore))
            {
                _diskProvider.SetFilePermissions(updateClientExePath, "755", null);
            }

            _logger.Info("Starting update client {0}", updateClientExePath);
            _logger.ProgressInfo("Sonarr will restart shortly.");

            _processProvider.Start(updateClientExePath, GetUpdaterArgs(updateSandboxFolder));

            return true;
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

        private UpdatePackage GetUpdatePackage(CommandTrigger updateTrigger)
        {
            _logger.ProgressDebug("Checking for updates");

            var latestAvailable = _checkUpdateService.AvailableUpdate();

            if (latestAvailable == null)
            {
                _logger.ProgressDebug("No update available");
                return null;
            }

            if (_osInfo.IsDocker)
            {
                _logger.ProgressDebug("Updating is disabled inside a docker container.  Please update the container image.");
                return null;
            }

            if (OsInfo.IsNotWindows && !_configFileProvider.UpdateAutomatically && updateTrigger != CommandTrigger.Manual)
            {
                _logger.ProgressDebug("Auto-update not enabled, not installing available update.");
                return null;
            }

            // Safety net, ConfigureUpdateMechanism should take care of invalid settings
            if (_configFileProvider.UpdateMechanism == UpdateMechanism.BuiltIn && _deploymentInfoProvider.IsExternalUpdateMechanism)
            {
                _logger.ProgressDebug("Built-In updater disabled, please use {0} to install", _deploymentInfoProvider.PackageUpdateMechanism);
                return null;
            }
            else if (_configFileProvider.UpdateMechanism != UpdateMechanism.Script && _deploymentInfoProvider.IsExternalUpdateMechanism)
            {
                _logger.ProgressDebug("Update available, please use {0} to install", _deploymentInfoProvider.PackageUpdateMechanism);
                return null;
            }

            return latestAvailable;
        }

        public void Execute(ApplicationUpdateCheckCommand message)
        {
            if (GetUpdatePackage(message.Trigger) != null)
            {
                _commandQueueManager.Push(new ApplicationUpdateCommand(), trigger: message.Trigger);
            }
        }

        public void Execute(ApplicationUpdateCommand message)
        {
            var latestAvailable = GetUpdatePackage(message.Trigger);

            if (latestAvailable != null)
            {
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

        public void Handle(ApplicationStartingEvent message)
        {
            // Check if we have to do an application update on startup

            try
            {
                var updateMarker = Path.Combine(_appFolderInfo.AppDataFolder, "update_required");
                if (!_diskProvider.FileExists(updateMarker))
                {
                    return;
                }

                _logger.Debug("Post-install update check requested");

                // Don't do a prestartup update check unless BuiltIn update is enabled
                if (!_configFileProvider.UpdateAutomatically ||
                    _configFileProvider.UpdateMechanism != UpdateMechanism.BuiltIn ||
                    _deploymentInfoProvider.IsExternalUpdateMechanism)
                {
                    _logger.Debug("Built-in updater disabled, skipping post-install update check");
                    return;
                }

                var latestAvailable = _checkUpdateService.AvailableUpdate();
                if (latestAvailable == null)
                {
                    _logger.Debug("No post-install update available");
                    _diskProvider.DeleteFile(updateMarker);
                    return;
                }

                _logger.Info("Installing post-install update from {0} to {1}", BuildInfo.Version, latestAvailable.Version);
                _diskProvider.DeleteFile(updateMarker);

                var installing = InstallUpdate(latestAvailable);

                if (installing)
                {
                    _logger.Debug("Install in progress, giving installer 30 seconds.");

                    var watch = Stopwatch.StartNew();

                    while (watch.Elapsed < TimeSpan.FromSeconds(30))
                    {
                        Thread.Sleep(1000);
                    }

                    _logger.Error("Post-install update not completed within 30 seconds. Attempting to continue normal operation.");
                }
                else
                {
                    _logger.Debug("Post-install update cancelled for unknown reason. Attempting to continue normal operation.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to perform the post-install update check. Attempting to continue normal operation.");
            }
        }
    }
}
