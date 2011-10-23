using System.IO;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Update.Providers
{
    public class UpdateProvider
    {
        private readonly DiskProvider _diskProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly ConsoleProvider _consoleProvider;
        private readonly ServiceProvider _serviceProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UpdateProvider(DiskProvider diskProvider, EnviromentProvider enviromentProvider, ConsoleProvider consoleProvider,
            ServiceProvider serviceProvider)
        {
            _diskProvider = diskProvider;
            _enviromentProvider = enviromentProvider;
            _consoleProvider = consoleProvider;
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            var sandboxFolder = Path.Combine(_enviromentProvider.StartUpPath, "nzbdrone_update");
            Logger.Info("Looking for update package at {0}", sandboxFolder);

            Logger.Info("Verifying Update Folder");
            if (!_diskProvider.FolderExists(sandboxFolder))
            {
                Logger.Error("Update folder doesn't exist {0}", sandboxFolder);
                _consoleProvider.UpdateFolderDoestExist(sandboxFolder);
                return;
            }

            if (_serviceProvider.ServiceExist(ServiceProvider.NzbDroneServiceName))
            {
                _serviceProvider.Stop(ServiceProvider.NzbDroneServiceName);
            }

            //Stop NzbDrone Process

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
