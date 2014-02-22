using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        Series FindByTitle(string cleanTitle);
        Series FindByTitle(string cleanTitle, int year);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Series FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .SingleOrDefault();
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .AndWhere(s => s.Year == year)
                        .SingleOrDefault();
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query.Where(s => s.TvdbId == tvdbId).SingleOrDefault();
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query.Where(s => s.TvRageId == tvRageId).SingleOrDefault();
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesType)
        {
            SetFields(new Series { Id = seriesId, SeriesType = seriesType }, s => s.SeriesType);
        }
    }
}