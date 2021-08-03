using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusRepository : IProviderStatusRepository<IndexerStatus>
    {
    }

    public class IndexerStatusRepository : ProviderStatusRepository<IndexerStatus>, IIndexerStatusRepository
    {
        public IndexerStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
