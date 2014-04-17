using System.Linq;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class IndexerCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerCheck(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public override HealthCheck Check()
        {
            var enabled = _indexerFactory.GetAvailableProviders();

            if (!enabled.Any())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "No indexers are enabled");
            }

            if (enabled.All(i => i.SupportsSearching == false))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not support searching");
            }

            return new HealthCheck(GetType());
        }
    }
}
