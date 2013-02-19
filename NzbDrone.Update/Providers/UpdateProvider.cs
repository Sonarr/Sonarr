using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Update.Providers
{
    public class UpdateProvider
    {
        private readonly DiskProvider _diskProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly ProcessProvider _processProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly HostController _hostController;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider, ServiceProvider serviceProvider,
            ProcessProvider processProvider, EnvironmentProvider environmentProvider, HostController hostController)
        {
            _diskProvider = diskProvider;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _environmentProvider = environmentProvider;
            _hostController = hostController;
        }

        public UpdateProvider()
        {
        }

        private void Verify(string targetFolder)
        {
            logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(targetFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist " + targetFolder);

            logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_environmentProvider.GetUpdatePackageFolder()))
                throw new DirectoryNotFoundException("Update folder doesn't exist " + _environmentProvider.GetUpdatePackageFolder());
        }

        public virtual void Start(string targetFolder)
        {
            Verify(targetFolder);
            AppType appType = AppType.Normal;

            logger.Info("Stopping all running services");

            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)
               && _serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                appType = AppType.Service;
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                appType = AppType.Normal;
            }

            //TODO:Should be able to restart service if anything beyond this point fails
            logger.Info("Killing all running processes");
            var processes = _processProvider.GetProcessByName(ProcessProvider.NzbDroneProccessName);
            foreach (var processInfo in processes)
            {
                _processProvider.Kill(processInfo.Id);
            }

            var consoleProcesses = _processProvider.GetProcessByName(ProcessProvider.NzbDroneConsoleProccessName);
            foreach (var processInfo in consoleProcesses)
            {
                appType = AppType.Console;
                _processProvider.Kill(processInfo.Id);
            }

            logger.Info("Killing all orphan IISExpress processes");
            _hostController.StopServer();

            logger.Info("Creating backup of existing installation");
            _diskProvider.CopyDirectory(targetFolder, _environmentProvider.GetUpdateBackUpFolder());

            logger.Info("Moving update package to target");

            try
            {
                _diskProvider.CopyDirectory(_environmentProvider.GetUpdatePackageFolder(), targetFolder);

                logger.Trace("Deleting Update Package.");
                _diskProvider.DeleteFolder(_environmentProvider.GetUpdatePackageFolder(), true);
            }
            catch (Exception e)
            {
                RollBack(targetFolder);

                foreach (var key in e.Data.Keys)
                {
                    logger.Trace("Key: {0}, Value: {1}", key, e.Data[key]);
                }

                logger.FatalException("Failed to copy upgrade package to target folder.", e);
            }
            finally
            {
                StartNzbDrone(appType, targetFolder);
            }
        }

        private void RollBack(string targetFolder)
        {
            //TODO:this should ignore single file failures.
            logger.Info("Attempting to rollback upgrade");
            _diskProvider.CopyDirectory(_environmentProvider.GetUpdateBackUpFolder(), targetFolder);
        }

        private void StartNzbDrone(AppType appType, string targetFolder)
        {
            logger.Info("Starting NzbDrone");
            if (appType == AppType.Service)
            {
                logger.Info("Starting NzbDrone service");
                _serviceProvider.Start(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }
            else if(appType == AppType.Console)
            {
                logger.Info("Starting NzbDrone with Console");
                _processProvider.Start(Path.Combine(targetFolder, "NzbDrone.Console.exe"));
            }
            else
            {
                logger.Info("Starting NzbDrone without Console");
                _processProvider.Start(Path.Combine(targetFolder, "NzbDrone.exe"));
            }
        }
    }
}
