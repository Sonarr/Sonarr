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
        private readonly EnviromentProvider _enviromentProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider,ServiceProvider serviceProvider,
            ProcessProvider processProvider, EnviromentProvider enviromentProvider)
        {
            _diskProvider = diskProvider;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _enviromentProvider = enviromentProvider;
        }

        private void Verify(string targetFolder)
        {
            logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(targetFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist " + targetFolder);

            logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_enviromentProvider.GetUpdatePackageFolder()))
                throw new DirectoryNotFoundException("Update folder doesn't exist " + _enviromentProvider.GetUpdatePackageFolder());

        }

        public void Start(string targetFolder)
        {
            Verify(targetFolder);
            bool isService = false;

            logger.Info("Stopping all running services");
            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                if (_serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
                {
                   isService = true; 
                }
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            logger.Info("Killing all running processes");
            var processes = _processProvider.GetProcessByName(ProcessProvider.NzbDroneProccessName);
            foreach (var processInfo in processes)
            {
                _processProvider.Kill(processInfo.Id);
            }

            logger.Info("Creating backup of existing installation");
            _diskProvider.CopyDirectory(targetFolder, _enviromentProvider.GetUpdateBackUpFolder());


            logger.Info("Copying update package to target");

            try
            {
                _diskProvider.CopyDirectory(_enviromentProvider.GetUpdatePackageFolder(), targetFolder);
            }
            catch (Exception e)
            {
                RollBack(targetFolder);
                logger.Fatal("Failed to copy upgrade package to target folder.", e);
            }
            finally
            {
                StartNzbDrone(isService, targetFolder);
            }
        }

        private void RollBack(string targetFolder)
        {
            logger.Info("Attempting to rollback upgrade");
            _diskProvider.CopyDirectory(_enviromentProvider.GetUpdateBackUpFolder(), targetFolder);
        }


        private void StartNzbDrone(bool isService, string targetFolder)
        {
            if (isService)
            {
                _serviceProvider.Start(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }
            else
            {
                _processProvider.Start(Path.Combine(targetFolder, "nzbdrone.exe"));
            }
        }
    }
}
