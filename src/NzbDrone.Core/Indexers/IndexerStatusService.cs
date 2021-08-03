using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusService : IProviderStatusServiceBase<IndexerStatus>
    {
        ReleaseInfo GetLastRssSyncReleaseInfo(int indexerId);

        void UpdateRssSyncStatus(int indexerId, ReleaseInfo releaseInfo);
    }

    public class IndexerStatusService : ProviderStatusServiceBase<IIndexer, IndexerStatus>, IIndexerStatusService
    {
        public IndexerStatusService(IIndexerStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public ReleaseInfo GetLastRssSyncReleaseInfo(int indexerId)
        {
            return GetProviderStatus(indexerId).LastRssSyncReleaseInfo;
        }

        public void UpdateRssSyncStatus(int indexerId, ReleaseInfo releaseInfo)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(indexerId);

                status.LastRssSyncReleaseInfo = releaseInfo;

                _providerStatusRepository.Upsert(status);
            }
        }
    }
}
