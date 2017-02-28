using System.Linq;
using NzbDrone.Common.Extensions;
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
                if (_indexerFactory.RssEnabled(false).Empty())
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not have RSS sync enabled");
                }

                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers with RSS sync enabled are disabled due to recent failures");
            }

            if (searchEnabled.Empty())
            {
                if (_indexerFactory.SearchEnabled(false).Empty())
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers do not have searching enabled");
                }

                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Enabled indexers with searching enabled are disabled due to recent failures");
            }

            return new HealthCheck(GetType());
        }
    }
}
