using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderAddedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IIndexer>))]
    public class IndexerRssCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerRssCheck(IIndexerFactory indexerFactory, ILocalizationService localizationService)
            : base(localizationService)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var enabled = _indexerFactory.RssEnabled(false);

            if (enabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, _localizationService.GetLocalizedString("IndexerRssNoIndexersEnabledHealthCheckMessage"), "#no-indexers-available-with-rss-sync-enabled-sonarr-will-not-grab-new-releases-automatically");
            }

            var active = _indexerFactory.RssEnabled(true);

            if (active.Empty())
            {
                 return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("IndexerRssNoIndexersAvailableHealthCheckMessage"), "#indexers-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType());
        }
    }
}
