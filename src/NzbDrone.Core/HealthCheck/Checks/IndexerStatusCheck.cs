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
            var backOffIndexers = enabledIndexers
                .Select(v => Tuple.Create(v, _indexerStatusService.GetIndexerStatus(v.Definition.Id)))
                .Where(v => v.Item2 != null && v.Item2.BackOffDate.HasValue && v.Item2.BackOffDate > DateTime.UtcNow &&
                      (v.Item2.LastFailure - v.Item2.FirstFailure) > TimeSpan.FromHours(1))
                .ToList();

            if (backOffIndexers.Empty())
            {
                return new HealthCheck(GetType());
            }

            if (backOffIndexers.Count == enabledIndexers.Count)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, "All indexers unusable due to failures", "#indexer-unusable-due-to-failures");
            }

            if (backOffIndexers.Count > 1)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "Multiple indexers unusable due to failures", "#indexer-unusable-due-to-failures");
            }

            var indexer = backOffIndexers.First();
            return new HealthCheck(GetType(), HealthCheckResult.Warning, string.Format("Indexer {0} unusable due to failures", indexer.Item1.Definition.Name), "#indexer-unusable-due-to-failures");
        }
    }
}
