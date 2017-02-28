using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class IndexerSearchCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerSearchCheck(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var enabled = _indexerFactory.SearchEnabled(false);

            if (enabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "No indexers available with Search enabled, Sonarr will not provide any search results");
            }

            var active = _indexerFactory.SearchEnabled(true);

            if (active.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "All search-capable indexers are temporarily unavailable due to recent indexer errors");
            }

            return new HealthCheck(GetType());
        }
    }
}
