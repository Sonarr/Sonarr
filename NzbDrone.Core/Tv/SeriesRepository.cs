using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common;
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
        public SeriesRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Queryable.Any(s => DiskProvider.PathEquals(s.Path, path));
        }

        public List<Series> Search(string title)
        {
            return Queryable.Where(s => s.Title.Contains(title)).ToList();
        }

        public Series GetByTitle(string cleanTitle)
        {
            return Queryable.SingleOrDefault(s => s.CleanTitle.Equals(cleanTitle));
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Queryable.SingleOrDefault(s => s.TvDbId == tvdbId);
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesTypes)
        {
            ObjectDatabase.UpdateField(new Series(){Id = seriesId, SeriesTypes =seriesTypes }, "SeriesType");
        }
    }
}