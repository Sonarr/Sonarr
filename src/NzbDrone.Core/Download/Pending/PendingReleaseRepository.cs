using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseRepository : IBasicRepository<PendingRelease>
    {
        void DeleteBySeriesId(Int32 seriesId);
        List<PendingRelease> AllBySeriesId(Int32 seriesId);
        void DeleteByMovieId(Int32 movieid);
        List<PendingRelease> AllByMovieId(Int32 movieId);
    }

    public class PendingReleaseRepository : BasicRepository<PendingRelease>, IPendingReleaseRepository
    {
        public PendingReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public new PendingRelease Insert(PendingRelease item)
        {
            Debug.Assert(!(item.MovieId > 0 && item.SeriesId > 0));
            return base.Insert(item);
        }

        public new void InsertMany(IList<PendingRelease> models)
        {
            Debug.Assert(models.Any(m => !(m.SeriesId > 0 && m.MovieId > 0)));
            base.InsertMany(models);
        }

        public void DeleteBySeriesId(Int32 seriesId)
        {
            Delete(r => r.SeriesId == seriesId);
        }

        public List<PendingRelease> AllBySeriesId(Int32 seriesId)
        {
            return Query.Where(p => p.SeriesId == seriesId);
        }

        public void DeleteByMovieId(Int32 movieId)
        {
            Delete(r => r.MovieId == movieId);
        }

        public List<PendingRelease> AllByMovieId(Int32 movieId)
        {
            return Query.Where(p => p.MovieId == movieId);
        }
    }
}