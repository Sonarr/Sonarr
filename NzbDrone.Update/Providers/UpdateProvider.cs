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
        private readonly EnviromentProvider _enviromentProvider;
        private readonly ServiceProvider _serviceProvider;
        private readonly ProcessProvider _processProvider;
        private readonly PathProvider _pathProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider, EnviromentProvider enviromentProvider,
            ServiceProvider serviceProvider, ProcessProvider processProvider, PathProvider pathProvider)
        {
            _diskProvider = diskProvider;
            _enviromentProvider = enviromentProvider;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _pathProvider = pathProvider;
        }

        private void Verify(string installationFolder)
        {
            Logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(installationFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(installationFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist" + installationFolder);

            Logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(_pathProvider.UpdatePackageFolder))
                throw new DirectoryNotFoundException("Update folder doesn't exist" + _pathProvider.UpdateSandboxFolder);

        }

        public void Start(string targetFolder)
        {
            Verify(targetFolder);
            bool isService = false;

            Logger.Info("Stopping all running services");
            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                if (_serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
                {
                   isService = true; 
                }
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            Logger.Info("Killing all running processes");
            var processes = _processProvider.GetProcessByName(ProcessProvider.NzbDroneProccessName);
            foreach (var processInfo in processes)
            {
                _processProvider.Kill(processInfo.Id);
            }

            Logger.Info("Creating backup of existing installation");
            _diskProvider.CopyDirectory(targetFolder, _pathProvider.UpdateBackUpFolder);


            Logger.Info("Copying update package to target");

            try
            {
                _diskProvider.CopyDirectory(_pathProvider.UpdatePackageFolder, targetFolder);
            }
            catch (Exception e)
            {
                RollBack(targetFolder);
                Logger.Fatal("Failed to copy upgrade package to target folder.", e);
            }
            finally
            {
                StartNzbDrone(isService, targetFolder);
            }
        }

        private void RollBack(string targetFolder)
        {
            Logger.Info("Attempting to rollback upgrade");
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
