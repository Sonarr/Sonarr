using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        Task DeleteBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
        Task<List<PendingRelease>> AllBySeriesIdAsync(int seriesId, CancellationToken cancellationToken = default);
        Task<List<PendingRelease>> WithoutFallbackAsync(CancellationToken cancellationToken = default);
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<List<PendingRelease>> AllBySeriesIdAsync(int seriesId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(p => p.SeriesId == seriesId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<PendingRelease>> WithoutFallbackAsync(CancellationToken cancellationToken = default)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .InnerJoin<PendingRelease, Series>((p, s) => p.SeriesId == s.Id)
                .Where<PendingRelease>(p => p.Reason != PendingReleaseReason.Fallback);

            return await QueryAsync(builder, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteBySeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(r => seriesIds.Contains(r.SeriesId), cancellationToken).ConfigureAwait(false);
        }
    }
}
