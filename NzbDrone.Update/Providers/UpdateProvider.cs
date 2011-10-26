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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider, EnviromentProvider enviromentProvider,
            ServiceProvider serviceProvider, ProcessProvider processProvider)
        {
            _diskProvider = diskProvider;
            _enviromentProvider = enviromentProvider;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public void Verify(string targetFolder)
        {
            Logger.Info("Verifying requirements before update...");

            if (String.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder can not be null or empty");

            if (!_diskProvider.FolderExists(targetFolder))
                throw new DirectoryNotFoundException("Target folder doesn't exist" + targetFolder);

            var sandboxFolder = Path.Combine(_enviromentProvider.StartUpPath, "nzbdrone_update");

            Logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(sandboxFolder))
                throw new DirectoryNotFoundException("Update folder doesn't exist" + sandboxFolder);

        }

        public void Start(string installationFolder)
        {
            Logger.Info("Stopping all running services");
            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            Logger.Info("Killing all running processes");
            var processes = _processProvider.GetProcessByName(ProcessProvider.NzbDroneProccessName);
            foreach (var processInfo in processes)
            {
                _processProvider.Kill(processInfo.Id);
            }


            //Create backup of current folder

            //Copy update folder on top of the existing folder

            //Happy: Start Service, Process?
            //Happy: Cleanup

            //Sad: delete fucked up folder
            //Sad: restore backup
            //Sad: start service, process
        }
    }
}
