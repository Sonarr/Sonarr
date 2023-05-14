using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Torznab;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderStatusChangedEvent<IIndexer>))]
    public class IndexerJackettAllCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _providerFactory;

        public IndexerJackettAllCheck(IIndexerFactory providerFactory)
        {
            _providerFactory = providerFactory;
        }

        public override HealthCheck Check()
        {
            var jackettAllProviders = _providerFactory.All()
                .Where(
                    i => i.ConfigContract.Equals("TorznabSettings") &&
                         (((TorznabSettings)i.Settings).BaseUrl.Contains("/torznab/all/api", StringComparison.InvariantCultureIgnoreCase) ||
                          ((TorznabSettings)i.Settings).BaseUrl.Contains("/api/v2.0/indexers/all/results/torznab", StringComparison.InvariantCultureIgnoreCase) ||
                          ((TorznabSettings)i.Settings).ApiPath.Contains("/torznab/all/api", StringComparison.InvariantCultureIgnoreCase) ||
                          ((TorznabSettings)i.Settings).ApiPath.Contains("/api/v2.0/indexers/all/results/torznab", StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();

            if (jackettAllProviders.Empty())
            {
                return new HealthCheck(GetType());
            }

            return new HealthCheck(GetType(),
                HealthCheckResult.Warning,
                string.Format("Indexers using the unsupported Jackett 'all' endpoint: {0}",
                    string.Join(", ", jackettAllProviders.Select(i => i.Name))),
                "#jackett-all-endpoint-used");
        }
    }
}
