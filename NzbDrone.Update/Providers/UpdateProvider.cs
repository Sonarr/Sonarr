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
        private readonly PathProvider _pathProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider,ServiceProvider serviceProvider,
            ProcessProvider processProvider, PathProvider pathProvider)
        {
            _diskProvider = diskProvider;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _pathProvider = pathProvider;
        }

        private void Verify(string installationFolder)
        {
            logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(installationFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(installationFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist" + installationFolder);

            logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_pathProvider.UpdatePackageFolder))
                throw new DirectoryNotFoundException("Update folder doesn't exist" + _pathProvider.UpdateSandboxFolder);

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
            _diskProvider.CopyDirectory(targetFolder, _pathProvider.UpdateBackUpFolder);


            logger.Info("Copying update package to target");

            try
            {
                _diskProvider.CopyDirectory(_pathProvider.UpdatePackageFolder, targetFolder);
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
            _diskProvider.CopyDirectory(_pathProvider.UpdateBackUpFolder, targetFolder);
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
