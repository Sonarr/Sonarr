using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ConfigSavedEvent))]
    public class ImportMechanismCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IProvideDownloadClient _provideDownloadClient;

        public ImportMechanismCheck(IConfigService configService, IProvideDownloadClient provideDownloadClient)
        {
            _configService = configService;
            _provideDownloadClient = provideDownloadClient;
        }

        public override HealthCheck Check()
        {
            List<ImportMechanismCheckStatus> downloadClients;

            try
            {
                downloadClients = _provideDownloadClient.GetDownloadClients().Select(v => new ImportMechanismCheckStatus
                {
                    DownloadClient = v,
                    Status = v.GetStatus()
                }).ToList();
            }
            catch (Exception)
            {
                // One or more download clients failed, assume the health is okay and verify later
                return new HealthCheck(GetType());
            }

            var downloadClientIsLocalHost = downloadClients.All(v => v.Status.IsLocalhost);

            if (!_configService.IsDefined("EnableCompletedDownloadHandling"))
            {
                // Migration helper logic
                if (!downloadClientIsLocalHost)
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Multi-Computer unsupported)", "#completedfailed-download-handling");
                }

                if (downloadClients.All(v => v.DownloadClient is Sabnzbd))
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Sabnzbd)", "#completedfailed-download-handling");
                }

                if (downloadClients.All(v => v.DownloadClient is Nzbget))
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible (Nzbget)", "#completedfailed-download-handling");
                }

                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling if possible", "#completedfailed-download-handling");
            }

            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enable Completed Download Handling");
            }

            return new HealthCheck(GetType());
        }
    }

    public class ImportMechanismCheckStatus
    {
        public IDownloadClient DownloadClient { get; set; }
        public DownloadClientInfo Status { get; set; }
    }
}
