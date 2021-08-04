using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteBySeriesId(int seriesId);
        List<PendingRelease> AllBySeriesId(int seriesId);
        List<PendingRelease> WithoutFallback();
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteBySeriesId(int seriesId)
        {
            Delete(r => r.SeriesId == seriesId);
        }

        public List<PendingRelease> AllBySeriesId(int seriesId)
        {
            return Query(p => p.SeriesId == seriesId);
        }

        public List<PendingRelease> WithoutFallback()
        {
            return Query(p => p.Reason != PendingReleaseReason.Fallback);
        }
    }
}
