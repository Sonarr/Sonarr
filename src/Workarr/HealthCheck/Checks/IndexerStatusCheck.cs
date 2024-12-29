using Workarr.Extensions;
using Workarr.Indexers;
using Workarr.Localization;
using Workarr.ThingiProvider.Events;

namespace Workarr.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IIndexer>))]
    public class IndexerStatusCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _providerFactory;
        private readonly IIndexerStatusService _providerStatusService;

        public IndexerStatusCheck(IIndexerFactory providerFactory, IIndexerStatusService providerStatusService, ILocalizationService localizationService)
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
                .Where(p => p.Status.InitialFailure.HasValue &&
                            p.Status.InitialFailure.Value.After(DateTime.UtcNow.AddHours(-6)))
                .ToList();

            if (backOffProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffProviders.Count == enabledProviders.Count)
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Error,
                    _localizationService.GetLocalizedString("IndexerStatusAllUnavailableHealthCheckMessage"),
                    "#indexers-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Warning,
                _localizationService.GetLocalizedString("IndexerStatusUnavailableHealthCheckMessage", new Dictionary<string, object>
                {
                    { "indexerNames", string.Join(", ", backOffProviders.Select(v => v.Provider.Definition.Name)) }
                }),
                "#indexers-are-unavailable-due-to-failures");
        }
    }
}
