using System.Linq;
using NzbDrone.Common;
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
            var rssEnabled = _indexerFactory.RssEnabled();
            var searchEnabled = _indexerFactory.SearchEnabled();

            if (enabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "No indexers are enabled");
            }

            if (enabled.All(i => i.SupportsRss == false))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not support RSS sync");
            }

            if (enabled.All(i => i.SupportsSearch == false))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not support searching");
            }

            if (rssEnabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not have RSS sync enabled");
            }

            if (searchEnabled.Empty())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not have searching enabled");
            }

            return new HealthCheck(GetType());
        }
    }
}
