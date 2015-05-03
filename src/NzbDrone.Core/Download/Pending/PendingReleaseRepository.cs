using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteBySeriesId(Int32 seriesId);
        List<PendingRelease> AllBySeriesId(Int32 seriesId);
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteBySeriesId(Int32 seriesId)
        {
            Delete(r => r.SeriesId == seriesId);
        }

        public List<PendingRelease> AllBySeriesId(Int32 seriesId)
        {
            return Query.Where(p => p.SeriesId == seriesId);
        }
    }
}