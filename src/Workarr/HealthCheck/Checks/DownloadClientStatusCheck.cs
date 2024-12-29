using Workarr.Download;
using Workarr.Extensions;
using Workarr.Localization;
using Workarr.ThingiProvider.Events;

namespace Workarr.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IDownloadClient>))]
    public class DownloadClientStatusCheck : HealthCheckBase
    {
        private readonly IDownloadClientFactory _providerFactory;
        private readonly IDownloadClientStatusService _providerStatusService;

        public DownloadClientStatusCheck(IDownloadClientFactory providerFactory, IDownloadClientStatusService providerStatusService, ILocalizationService localizationService)
            : base(localizationService)
        {
            _providerFactory = providerFactory;
            _providerStatusService = providerStatusService;
        }

        public override HealthCheck Check()
        {
            var enabledProviders = _providerFactory.GetAvailableProviders();
            var backOffProviders = enabledProviders.Join(_providerStatusService.GetBlockedProviders(),
                    i => i.Definition.Id,
                    s => s.ProviderId,
                    (i, s) => new { Provider = i, Status = s })
                .ToList();

            if (backOffProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffProviders.Count == enabledProviders.Count)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("DownloadClientStatusAllClientHealthCheckMessage"),
                    "#download-clients-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Warning,
                _localizationService.GetLocalizedString("DownloadClientStatusSingleClientHealthCheckMessage", new Dictionary<string, object>
                {
                    { "downloadClientNames", string.Join(", ", backOffProviders.Select(v => v.Provider.Definition.Name)) }
                }),
                "#download-clients-are-unavailable-due-to-failures");
        }
    }
}
