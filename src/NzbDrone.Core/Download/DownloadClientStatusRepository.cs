using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Download
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
