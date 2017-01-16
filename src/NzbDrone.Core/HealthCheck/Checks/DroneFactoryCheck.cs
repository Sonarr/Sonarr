using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class DroneFactoryCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;

        public DroneFactoryCheck(IConfigService configService, IDiskProvider diskProvider)
        {
            _configService = configService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var droneFactoryFolder = _configService.DownloadedEpisodesFolder;

            if (droneFactoryFolder.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            if (!_diskProvider.FolderExists(droneFactoryFolder))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Drone factory folder does not exist");
            }
            
            if (!_diskProvider.FolderWritable(droneFactoryFolder))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "Unable to write to drone factory folder");
            }

            //Todo: Unable to import one or more files/folders from

            return new HealthCheck(GetType());
        }
    }
}
