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
        DateTime GetBackOffDate(int indexerId);
        void ReportSuccess(int indexerId);
        void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan));

        DateTime? GetLastRecentSearch(int indexerId);
        void UpdateLastRecentSearch(int indexerId);
        ReleaseInfo GetLastRecentReleaseInfo(int indexerId);
        void UpdateLastRecentReleaseInfo(int indexerId, ReleaseInfo releaseInfo, bool fullyUpdated);
    }

    public class IndexerStatusService : IIndexerStatusService
    {
        const int MinimumBackOffPeriod = 5;
        const int MaximumBackOffPeriod = 86400;
        const int MaximumEscalation = 17;

        private static readonly object _syncRoot = new object();

        private readonly IIndexerStatusRepository _indexerStatusRepository;
        private readonly Logger _logger;

        public IndexerStatusService(IIndexerStatusRepository indexerStatusRepository, Logger logger)
        {
            _indexerStatusRepository = indexerStatusRepository;
            _logger = logger;
        }

        private IndexerStatus GetIndexerStatus(int indexerId)
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

        public DateTime GetBackOffDate(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            if (status.FailureEscalation == 0 || !status.LastFailure.HasValue)
            {
                return DateTime.UtcNow;
            }

            return status.LastFailure.Value + CalculateBackOffPeriod(status);
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

                _indexerStatusRepository.Upsert(status);
            }
        }

        public void ReportFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan))
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                if (status.FailureEscalation == 0)
                {
                    status.FirstFailure = DateTime.UtcNow;
                }

                status.LastFailure = DateTime.UtcNow;
                status.FailureEscalation = Math.Min(MaximumEscalation, status.FailureEscalation + 1);

                if (minimumBackOff != TimeSpan.Zero)
                {
                    while (status.FailureEscalation != MaximumEscalation && CalculateBackOffPeriod(status) < minimumBackOff)
                    {
                        status.FailureEscalation++;
                    }
                }

                _indexerStatusRepository.Upsert(status);
            }
        }

        public DateTime? GetLastRecentSearch(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            return status.LastRecentSearch;
        }

        public void UpdateLastRecentSearch(int indexerId)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                status.LastRecentSearch = DateTime.UtcNow;

                _indexerStatusRepository.Upsert(status);
            }
        }

        public ReleaseInfo GetLastRecentReleaseInfo(int indexerId)
        {
            var status = GetIndexerStatus(indexerId);

            return status.LastRecentReleaseInfo;
        }

        public void UpdateLastRecentReleaseInfo(int indexerId, ReleaseInfo releaseInfo, bool fullyUpdated)
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
