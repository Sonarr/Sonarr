using Workarr.Datastore;
using Workarr.Messaging.Events;
using Workarr.ThingiProvider.Status;

namespace Workarr.Download
{
    public interface IDownloadClientStatusRepository : IProviderStatusRepository<DownloadClientStatus>
    {
    }

    public class DownloadClientStatusRepository : ProviderStatusRepository<DownloadClientStatus>, IDownloadClientStatusRepository
    {
        public DownloadClientStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
