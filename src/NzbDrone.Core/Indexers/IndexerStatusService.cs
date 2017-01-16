using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusService
    {
        List<IndexerStatus> GetBlockedIndexers();
        ReleaseInfo GetLastRssSyncReleaseInfo(int indexerId);
        void RecordSuccess(int indexerId);
        void RecordFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan));
        void RecordConnectionFailure(int indexerId);

        void UpdateRssSyncStatus(int indexerId, ReleaseInfo releaseInfo);
    }

    public class IndexerStatusService : IIndexerStatusService, IHandleAsync<ProviderDeletedEvent<IIndexer>>
    {
        private static readonly int[] EscalationBackOffPeriods = {
                                                                     0,
                                                                     5 * 60,
                                                                     15 * 60,
                                                                     30 * 60,
                                                                     60 * 60,
                                                                     3 * 60 * 60,
                                                                     6 * 60 * 60,
                                                                     12 * 60 * 60,
                                                                     24 * 60 * 60
                                                                 };
        private static readonly int MaximumEscalationLevel = EscalationBackOffPeriods.Length - 1;

        private static readonly object _syncRoot = new object();

        private readonly IIndexerStatusRepository _indexerStatusRepository;
        private readonly Logger _logger;

        public IndexerStatusService(IIndexerStatusRepository indexerStatusRepository, Logger logger)
        {
            _indexerStatusRepository = indexerStatusRepository;
            _logger = logger;
        }

        public List<IndexerStatus> GetBlockedIndexers()
        {
            return _indexerStatusRepository.All().Where(v => v.IsDisabled()).ToList();
        }

        public ReleaseInfo GetLastRssSyncReleaseInfo(int indexerId)
        {
            return GetIndexerStatus(indexerId).LastRssSyncReleaseInfo;
        }

        private IndexerStatus GetIndexerStatus(int indexerId)
        {
            return _indexerStatusRepository.FindByIndexerId(indexerId) ?? new IndexerStatus { IndexerId = indexerId };
        }

        private TimeSpan CalculateBackOffPeriod(IndexerStatus status)
        {
            var level = Math.Min(MaximumEscalationLevel, status.EscalationLevel);

            return TimeSpan.FromSeconds(EscalationBackOffPeriods[level]);
        }

        public void RecordSuccess(int indexerId)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                if (status.EscalationLevel == 0)
                {
                    return;
                }

                status.EscalationLevel--;
                status.DisabledTill = null;

                _indexerStatusRepository.Upsert(status);
            }
        }

        protected void RecordFailure(int indexerId, TimeSpan minimumBackOff, bool escalate)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                var now = DateTime.UtcNow;

                if (status.EscalationLevel == 0)
                {
                    status.InitialFailure = now;
                }

                status.MostRecentFailure = now;
                if (escalate)
                {
                    status.EscalationLevel = Math.Min(MaximumEscalationLevel, status.EscalationLevel + 1);
                }

                if (minimumBackOff != TimeSpan.Zero)
                {
                    while (status.EscalationLevel < MaximumEscalationLevel && CalculateBackOffPeriod(status) < minimumBackOff)
                    {
                        status.EscalationLevel++;
                    }
                }

                status.DisabledTill = now + CalculateBackOffPeriod(status);

                _indexerStatusRepository.Upsert(status);
            }
        }

        public void RecordFailure(int indexerId, TimeSpan minimumBackOff = default(TimeSpan))
        {
            RecordFailure(indexerId, minimumBackOff, true);
        }

        public void RecordConnectionFailure(int indexerId)
        {
            RecordFailure(indexerId, default(TimeSpan), false);
        }


        public void UpdateRssSyncStatus(int indexerId, ReleaseInfo releaseInfo)
        {
            lock (_syncRoot)
            {
                var status = GetIndexerStatus(indexerId);

                status.LastRssSyncReleaseInfo = releaseInfo;

                _indexerStatusRepository.Upsert(status);
            }
        }

        public void HandleAsync(ProviderDeletedEvent<IIndexer> message)
        {
            var indexerStatus = _indexerStatusRepository.FindByIndexerId(message.ProviderId);

            if (indexerStatus != null)
            {
                _indexerStatusRepository.Delete(indexerStatus);
            }
        }
    }
}
