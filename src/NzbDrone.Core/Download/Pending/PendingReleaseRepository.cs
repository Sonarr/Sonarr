using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteBySeriesIds(List<int> seriesIds);
        List<PendingRelease> AllBySeriesId(int seriesId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteBySeriesIds(List<int> seriesIds)
        {
            Delete(r => seriesIds.Contains(r.SeriesId));
        }

        public List<PendingRelease> AllBySeriesId(int seriesId)
        {
            return Query(p => p.SeriesId == seriesId);
        }

        public List<PendingRelease> WithoutFallback()
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .InnerJoin<PendingRelease, Series>((p, s) => p.SeriesId == s.Id)
                .Where<PendingRelease>(p => p.Reason != PendingReleaseReason.Fallback);

            return Query(builder);
        }
    }
}
