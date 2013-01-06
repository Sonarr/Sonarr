using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Jobs
{
    public class AppUpdateJob : IJob
    {
        private readonly UpdateProvider _updateProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly DiskProvider _diskProvider;
        private readonly HttpProvider _httpProvider;
        private readonly ProcessProvider _processProvider;
        private readonly ArchiveProvider _archiveProvider;
        private readonly ConfigFileProvider _configFileProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AppUpdateJob(UpdateProvider updateProvider, EnvironmentProvider environmentProvider, DiskProvider diskProvider,
            HttpProvider httpProvider, ProcessProvider processProvider, ArchiveProvider archiveProvider, ConfigFileProvider configFileProvider)
        {
            _updateProvider = updateProvider;
            _environmentProvider = environmentProvider;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _processProvider = processProvider;
            _archiveProvider = archiveProvider;
            _configFileProvider = configFileProvider;
        }

        public string Name
        {
            get { return "Update Application Job"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(2); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            notification.CurrentMessage = "Checking for updates";

            var updatePackage = _updateProvider.GetAvilableUpdate(_environmentProvider.Version);

            //No updates available
            if (updatePackage == null)
                return;

            var packageDestination = Path.Combine(_environmentProvider.GetUpdateSandboxFolder(), updatePackage.FileName);

            if (_diskProvider.FolderExists(_environmentProvider.GetUpdateSandboxFolder()))
            {
                logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(_environmentProvider.GetUpdateSandboxFolder(), true);
            }

            logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            notification.CurrentMessage = "Downloading Update " + updatePackage.Version;
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            logger.Info("Extracting Update package");
            notification.CurrentMessage = "Extracting Update";
            _archiveProvider.ExtractArchive(packageDestination, _environmentProvider.GetUpdateSandboxFolder());
            logger.Info("Update package extracted successfully");

            logger.Info("Preparing client");
            notification.CurrentMessage = "Preparing to start Update";
            _diskProvider.MoveDirectory(_environmentProvider.GetUpdateClientFolder(), _environmentProvider.GetUpdateSandboxFolder());


            logger.Info("Starting update client");
            var startInfo = new ProcessStartInfo
                                {
                                    FileName = _environmentProvider.GetUpdateClientExePath(),
                                    Arguments = string.Format("{0} {1}", _environmentProvider.NzbDroneProcessIdFromEnviroment, _configFileProvider.Guid)
                                };

            var process = _processProvider.Start(startInfo);
            notification.CurrentMessage = "Update in progress. NzbDrone will restart shortly.";

           _processProvider.WaitForExit(process);
        }
    }
}