using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteBySeriesId(int seriesId);
        List<PendingRelease> AllBySeriesId(int seriesId);
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
            return Query.Where(p => p.SeriesId == seriesId);
        }
    }
}