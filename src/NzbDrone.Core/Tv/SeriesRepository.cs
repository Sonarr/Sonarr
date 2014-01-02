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
        Series SearchByTitle(string cleanTitle);
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
            return Query.Any(c => c.Path == path);
        }

        public Series FindByTitle(string cleanTitle)
        {
            return Query.SingleOrDefault(s => s.CleanTitle.Equals(cleanTitle, StringComparison.InvariantCultureIgnoreCase));
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            return Query.SingleOrDefault(s => s.CleanTitle.Equals(cleanTitle, StringComparison.InvariantCultureIgnoreCase) &&
                                              s.Year == year);
        }

        public Series SearchByTitle(string cleanTitle)
        {
            if (string.IsNullOrWhiteSpace(cleanTitle))
            {
                return null;
            }

            // find exact match first
            Series exactMatch = FindByTitle(cleanTitle);
            if (exactMatch != null)
            {
                return exactMatch;
            }

            // do fuzzy search
            var list = All().Where(s => cleanTitle.Contains(s.CleanTitle)).ToList();
            return (list.Count == 1) ? list.Single() : null;
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query.SingleOrDefault(s => s.TvdbId.Equals(tvdbId));
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query.SingleOrDefault(s => s.TvRageId.Equals(tvRageId));
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesType)
        {
            SetFields(new Series { Id = seriesId, SeriesType = seriesType }, s => s.SeriesType);
        }
    }
}