using System.Collections.Generic;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        List<Series> Search(string title);
        Series GetByTitle(string cleanTitle);
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
            return Queryable().Any(c => c.Path == path);
        }

        public List<Series> Search(string title)
        {
            return Queryable().Where(s => s.Title.Contains(title));
        }

        public Series GetByTitle(string cleanTitle)
        {
            return Queryable().SingleOrDefault(s => s.CleanTitle.Equals(cleanTitle));
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Queryable().SingleOrDefault(s => s.TvDbId.Equals(tvdbId));
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesType)
        {
            UpdateFields(new Series { Id = seriesId, SeriesType = seriesType }, s => s.SeriesType);
        }
    }
}