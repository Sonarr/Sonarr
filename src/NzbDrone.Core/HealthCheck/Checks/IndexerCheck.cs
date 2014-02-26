using System.Linq;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class IndexerCheck : IProvideHealthCheck
    {
        private readonly IIndexerFactory _indexerFactory;

        public IndexerCheck(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public HealthCheck Check()
        {
            var enabled = _indexerFactory.GetAvailableProviders();

            if (!enabled.Any())
            {
                return new HealthCheck(HealthCheckResultType.Error, "No indexers are enabled");
            }

            

            if (enabled.All(i => i.SupportsSearching == false))
            {
                return new HealthCheck(HealthCheckResultType.Warning, "Enabled indexers do not support searching");
            }

            return null;
        }
    }
}
