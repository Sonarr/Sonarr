using System;
using System.IO;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Nzbget;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class ImportMechanismCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IProvideDownloadClient _provideDownloadClient;
        private readonly IDownloadTrackingService _downloadTrackingService;

        public ImportMechanismCheck(IConfigService configService, IProvideDownloadClient provideDownloadClient, IDownloadTrackingService downloadTrackingService)
        {
            _configService = configService;
            _provideDownloadClient = provideDownloadClient;
            _downloadTrackingService = downloadTrackingService;
        }

        public override HealthCheck Check()
        {
            var droneFactoryFolder = new OsPath(_configService.DownloadedEpisodesFolder);
            var downloadClients = _provideDownloadClient.GetDownloadClients().Select(v => new { downloadClient = v, status = v.GetStatus() }).ToList();

            var downloadClientIsLocalHost = downloadClients.All(v => v.status.IsLocalhost);
            var downloadClientOutputInDroneFactory = !droneFactoryFolder.IsEmpty
                && downloadClients.Any(v => v.status.OutputRootFolders != null && v.status.OutputRootFolders.Any(droneFactoryFolder.Contains));

            if (!_configService.IsDefined("EnableCompletedDownloadHandling"))
            {
                // Migration helper logic
                if (!downloadClientIsLocalHost)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Multi-Computer unsupported)", "Migrating-to-Completed-Download-Handling#Unsupported-download-client-on-different-computer");
                }

                if (downloadClients.All(v => v.downloadClient is Sabnzbd))
                {
                    // With Sabnzbd we can check if the category should be changed.
                    if (downloadClientOutputInDroneFactory)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Sabnzbd - Conflicting Category)", "Migrating-to-Completed-Download-Handling#sabnzbd-conflicting-download-client-category");
                    }

                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Sabnzbd)", "Migrating-to-Completed-Download-Handling#sabnzbd-enable-completed-download-handling");
                }
                else if (downloadClients.All(v => v.downloadClient is Nzbget))
                {
                    // With Nzbget we can check if the category should be changed.
                    if (downloadClientOutputInDroneFactory)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Nzbget - Conflicting Category)", "Migrating-to-Completed-Download-Handling#nzbget-conflicting-download-client-category");
                    }

                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Nzbget)", "Migrating-to-Completed-Download-Handling#nzbget-enable-completed-download-handling");
                }
                else
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible", "Migrating-to-Completed-Download-Handling");
                }
            }

            if (!_configService.EnableCompletedDownloadHandling && droneFactoryFolder.IsEmpty)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling or configure Drone factory");
            }

            if (_configService.EnableCompletedDownloadHandling && !droneFactoryFolder.IsEmpty)
            {
                if (_downloadTrackingService.GetCompletedDownloads().Any(v => droneFactoryFolder.Contains(v.DownloadItem.OutputPath)))
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Completed Download Handling conflict with Drone Factory (Conflicting History Item)", "Migrating-to-Completed-Download-Handling#conflicting-download-client-category");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
