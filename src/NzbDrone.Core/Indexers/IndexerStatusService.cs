using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusService
    {
        IndexerStatus GetIndexerStatus(int indexerId);
        void ReportSuccess(int indexerId);
        void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan));

        void UpdateRecentSearchStatus(int indexerId, ReleaseInfo releaseInfo, bool fullyUpdated);
    }

    public class IndexerStatusService : IIndexerStatusService
    {
        const int MinimumBackOffPeriod = 5 * 60;
        const int MaximumBackOffPeriod = 24 * 60 * 60;
        const int MaximumEscalation = 10;

        private static readonly object _syncRoot = new object();

        private readonly IIndexerStatusRepository _indexerStatusRepository;
        private readonly Logger _logger;

        public IndexerStatusService(IIndexerStatusRepository indexerStatusRepository, Logger logger)
        {
            _indexerStatusRepository = indexerStatusRepository;
            _logger = logger;
        }

        public IndexerStatus GetIndexerStatus(int indexerId)
        {
            return _indexerStatusRepository.FindByIndexerId(indexerId) ?? new IndexerStatus { IndexerId = indexerId };
        }

        private TimeSpan CalculateBackOffPeriod(IndexerStatus status)
        {
            if (status.FailureEscalation == 0 || !status.LastFailure.HasValue)
            {
                return TimeSpan.Zero;
            }

            var backOffPeriod = Math.Min(MaximumBackOffPeriod, MinimumBackOffPeriod << (status.FailureEscalation - 1));

            return TimeSpan.FromSeconds(backOffPeriod);
        }

        public void ReportSuccess(int indexerId)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                if (status.FailureEscalation == 0)
                {
                    return;
                }

                status.FailureEscalation--;
                status.BackOffDate = null;

                _indexerStatusRepository.Upsert(status);
            }
        }

        public void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan))
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                var now = DateTime.UtcNow;

                if (status.FailureEscalation == 0)
                {
                    status.FirstFailure = now;
                }

                status.LastFailure = now;
                status.FailureEscalation = Math.Min(MaximumEscalation, status.FailureEscalation + 1);

                if (minimumBackOff != TimeSpan.Zero)
                {
                    while (status.FailureEscalation != MaximumEscalation && CalculateBackOffPeriod(status) < minimumBackOff)
                    {
                        status.FailureEscalation++;
                    }
                }

                status.BackOffDate = now + CalculateBackOffPeriod(status);

                _indexerStatusRepository.Upsert(status);
            }
        }

        public void UpdateRecentSearchStatus(int indexerId, ReleaseInfo releaseInfo, bool fullyUpdated)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                if (fullyUpdated)
                {
                    status.LastRecentSearch = DateTime.UtcNow;
                }
                status.LastRecentReleaseInfo = releaseInfo;

                _indexerStatusRepository.Upsert(status);
            }
        }
    }
}
