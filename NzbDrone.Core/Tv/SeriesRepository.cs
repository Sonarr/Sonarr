using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        List<Series> Search(string title);
        Series FindByTitle(string cleanTitle);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
        Series FindBySlug(string slug);
        List<String> GetSeriesPaths();
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

        public List<Series> Search(string title)
        {
            return Query.Where(s => s.Title.Contains(title));
        }

        public Series FindByTitle(string cleanTitle)
        {
            return Query.SingleOrDefault(s => s.CleanTitle.Equals(cleanTitle, StringComparison.InvariantCultureIgnoreCase));
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

        public Series FindBySlug(string slug)
        {
            return Query.SingleOrDefault(c => c.TitleSlug == slug.ToLower());
        }

        public List<string> GetSeriesPaths()
        {
            return Query.Select(s => s.Path).ToList();
        }
    }
}