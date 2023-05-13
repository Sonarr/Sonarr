using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Localization;
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

        public ImportMechanismCheck(IConfigService configService, IProvideDownloadClient provideDownloadClient, ILocalizationService localizationService)
            : base(localizationService)
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

            if (!_configService.IsDefined("EnableCompletedDownloadHandling"))
            {
                var downloadClientIsLocalHost = downloadClients.All(v => v.Status.IsLocalhost);

                // Migration helper logic
                if (!downloadClientIsLocalHost)
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Warning,
                        _localizationService.GetLocalizedString("ImportMechanismEnableCompletedDownloadHandlingIfPossibleMultiComputerHealthCheckMessage"),
                        "#completedfailed-download-handling");
                }

                if (downloadClients.All(v => v.DownloadClient is Sabnzbd))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Warning,
                        $"{_localizationService.GetLocalizedString("ImportMechanismEnableCompletedDownloadHandlingIfPossibleHealthCheckMessage")} (Sabnzbd)",
                        "#completedfailed-download-handling");
                }

                if (downloadClients.All(v => v.DownloadClient is Nzbget))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Warning,
                        $"{_localizationService.GetLocalizedString("ImportMechanismEnableCompletedDownloadHandlingIfPossibleHealthCheckMessage")} (Nzbget)",
                        "#completedfailed-download-handling");
                }

                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    _localizationService.GetLocalizedString("ImportMechanismEnableCompletedDownloadHandlingIfPossibleHealthCheckMessage"),
                    "#completedfailed-download-handling");
            }

            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    _localizationService.GetLocalizedString("ImportMechanismHandlingDisabledHealthCheckMessage"),
                    "#completed-download-handling-is-disabled");
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
