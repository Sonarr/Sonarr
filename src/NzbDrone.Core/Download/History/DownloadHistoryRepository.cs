using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.History
{
    public interface IDownloadHistoryRepository : IBasicRepository<DownloadHistory>
    {
        Task<List<DownloadHistory>> FindByDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default);
        Task DeleteBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
    }

    public class DownloadHistoryRepository : BasicRepository<DownloadHistory>, IDownloadHistoryRepository
    {
        public DownloadHistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<List<DownloadHistory>> FindByDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default)
        {
            var downloadHistories = await QueryAsync(h => h.DownloadId == downloadId, cancellationToken).ConfigureAwait(false);
            return downloadHistories.OrderByDescending(h => h.Date).ToList();
        }

        public async Task DeleteBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(r => seriesIds.Contains(r.SeriesId), cancellationToken).ConfigureAwait(false);
        }
    }
}
