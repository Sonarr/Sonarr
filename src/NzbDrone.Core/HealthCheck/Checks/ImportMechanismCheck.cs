using System;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class ImportMechanismCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IDownloadTrackingService _downloadTrackingService;

        public ImportMechanismCheck(IConfigService configService, IDownloadTrackingService downloadTrackingService)
        {
            _configService = configService;
            _downloadTrackingService = downloadTrackingService;
        }

        public override HealthCheck Check()
        {
            if (!_configService.IsDefined("EnableCompletedDownloadHandling"))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Completed Download Handling is disabled");
            }

            var droneFactoryFolder = _configService.DownloadedEpisodesFolder;

            if (!_configService.EnableCompletedDownloadHandling && droneFactoryFolder.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling or configure Drone factory");
            }

            if (_configService.EnableCompletedDownloadHandling && !droneFactoryFolder.IsNullOrWhiteSpace() && _downloadTrackingService.GetCompletedDownloads().Any(v => droneFactoryFolder.PathEquals(v.DownloadItem.OutputPath) || droneFactoryFolder.IsParentPath(v.DownloadItem.OutputPath)))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Download Client has history items in Drone Factory conflicting with Completed Download Handling");
            }

            return new HealthCheck(GetType());
        }
    }
}
