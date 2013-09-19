using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Update.Commands;
using NzbDrone.Core.Instrumentation;

namespace NzbDrone.Core.Update
{
    public class InstallUpdateService : IExecute<ApplicationUpdateCommand>
    {
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly Logger _logger;
        private readonly IAppFolderInfo _appFolderInfo;

        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly IArchiveService _archiveService;
        private readonly IProcessProvider _processProvider;


        public InstallUpdateService(ICheckUpdateService checkUpdateService, IAppFolderInfo appFolderInfo,
                                    IDiskProvider diskProvider, IHttpProvider httpProvider,
                                    IArchiveService archiveService, IProcessProvider processProvider, Logger logger)
        {
            _checkUpdateService = checkUpdateService;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _archiveService = archiveService;
            _processProvider = processProvider;
            _logger = logger;
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

        private void InstallUpdate(UpdatePackage updatePackage)
        {
            try
            {
                var updateSandboxFolder = _appFolderInfo.GetUpdateSandboxFolder();

                var packageDestination = Path.Combine(updateSandboxFolder, updatePackage.FileName);

                if (_diskProvider.FolderExists(updateSandboxFolder))
                {
                    _logger.Info("Deleting old update files");
                    _diskProvider.DeleteFolder(updateSandboxFolder, true);
                }

                _logger.ProgressInfo("Downloading Updated {0} [{1}]", updatePackage.Version, updatePackage.Branch);
                _logger.Debug("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
                _httpProvider.DownloadFile(updatePackage.Url, packageDestination);

                _logger.ProgressInfo("Extracting Update package");
                _archiveService.Extract(packageDestination, updateSandboxFolder);
                _logger.Info("Update package extracted successfully");

                _logger.Info("Preparing client");
                _diskProvider.MoveFolder(_appFolderInfo.GetUpdateClientFolder(),
                                            updateSandboxFolder);

                _logger.Info("Starting update client {0}", _appFolderInfo.GetUpdateClientExePath());

                _logger.ProgressInfo("NzbDrone will restart shortly.");

                _processProvider.Start(_appFolderInfo.GetUpdateClientExePath(), _processProvider.GetCurrentProcess().Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Update process failed", ex);
            }
        }
    }
}
