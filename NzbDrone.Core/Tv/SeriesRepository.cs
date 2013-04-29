using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        List<Series> Search(string title);
        Series FindByTitle(string cleanTitle);
        Series FindByTvdbId(int tvdbId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IDatabase database)
            : base(database)
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

        public void SetSeriesType(int seriesId, SeriesTypes seriesType)
        {
            SetFields(new Series { Id = seriesId, SeriesType = seriesType }, s => s.SeriesType);
        }

    }
}