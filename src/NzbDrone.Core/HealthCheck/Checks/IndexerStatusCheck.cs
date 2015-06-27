using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class IndexerStatusCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IIndexerStatusService _indexerStatusService;

        public IndexerStatusCheck(IIndexerFactory indexerFactory, IIndexerStatusService indexerStatusService)
        {
            _indexerFactory = indexerFactory;
            _indexerStatusService = indexerStatusService;
        }

        public override HealthCheck Check()
        {
            var enabledIndexers = _indexerFactory.GetAvailableProviders();
            var backOffIndexers = enabledIndexers.Join(_indexerStatusService.GetBlockedIndexers(),
                    i => i.Definition.Id,
                    s => s.IndexerId,
                    (i, s) => new { Indexer = i, Status = s })
                .Where(v => (v.Status.MostRecentFailure - v.Status.InitialFailure) > TimeSpan.FromHours(1))
                .ToList();

            if (backOffIndexers.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffIndexers.Count == enabledIndexers.Count)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "All indexers are unavailable due to failures", "#indexers-are-unavailable-due-to-failures");
            }

            if (backOffIndexers.Count > 1)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("{0} indexers are unavailable due to failures", backOffIndexers.Count), "#indexers-are-unavailable-due-to-failures");
            }

            var indexer = backOffIndexers.First();
            return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Indexer {0} is unavailable due to failures", indexer.Indexer.Definition.Name), "#indexers-are-unavailable-due-to-failures");
        }
    }
}
