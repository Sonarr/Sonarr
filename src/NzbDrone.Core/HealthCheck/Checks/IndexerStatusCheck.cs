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
                    s => s.ProviderId,
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

            return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Indexers unavailable due to failures: {0}", string.Join(", ", backOffIndexers.Select(v => v.Indexer.Definition.Name))), "#indexers-are-unavailable-due-to-failures");
        }
    }
}
