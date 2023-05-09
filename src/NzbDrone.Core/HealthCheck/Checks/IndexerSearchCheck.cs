using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderAddedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IIndexer>))]
    public class IndexerSearchCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerSearchCheck(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var automaticSearchEnabled = _indexerFactory.AutomaticSearchEnabled(false);

            if (automaticSearchEnabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No indexers available with Automatic Search enabled, Sonarr will not provide any automatic search results", "#no-indexers-available-with-automatic-search-enabled-sonarr-will-not-provide-any-automatic-search-results");
            }

            var interactiveSearchEnabled = _indexerFactory.InteractiveSearchEnabled(false);

            if (interactiveSearchEnabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No indexers available with Interactive Search enabled, Sonarr will not provide any interactive search results", "#no-indexers-available-with-interactive-search-enabled");
            }

            var active = _indexerFactory.AutomaticSearchEnabled(true);

            if (active.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "All search-capable indexers are temporarily unavailable due to recent indexer errors", "#indexers-are-unavailable-due-to-failures");
            }

            return new HealthCheck(GetType());
        }
    }
}
