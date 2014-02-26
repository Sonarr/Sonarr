using System;
using System.IO;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DroneFactoryCheck : IProvideHealthCheck
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;

        public DroneFactoryCheck(IConfigService configService, IDiskProvider diskProvider)
        {
            _configService = configService;
            _diskProvider = diskProvider;
        }

        public HealthCheck Check()
        {
            var droneFactoryFolder = _configService.DownloadedEpisodesFolder;

            if (droneFactoryFolder.IsNullOrWhiteSpace())
            {
                return new HealthCheck(HealthCheckResultType.Warning, "Drone factory folder is not configured");
            }

            if (!_diskProvider.FolderExists(droneFactoryFolder))
            {
                return new HealthCheck(HealthCheckResultType.Error, "Drone factory folder does not exist");
            }
            
            try
            {
                var testPath = Path.Combine(droneFactoryFolder, "drone_test.txt");
                _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
                _diskProvider.DeleteFile(testPath);
            }
            catch (Exception)
            {
                return new HealthCheck(HealthCheckResultType.Error, "Unable to write to drone factory folder");
            }

            //Todo: Unable to import one or more files/folders from

            return null;
        }
    }
}
