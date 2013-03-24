using System.Collections.Generic;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;
using ServiceStack.OrmLite;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicDb<Series>
    {
        bool SeriesPathExists(string path);
        List<Series> Search(string title);
        Series GetByTitle(string cleanTitle);
        Series FindByTvdbId(int tvdbId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
    }

    public class SeriesRepository : BasicDb<Series>, ISeriesRepository
    {
        public SeriesRepository(IDbConnection database)
            : base(database)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Database.Exists<Series>("WHERE Path = {0}", path);
        }

        public List<Series> Search(string title)
        {
            return Database.Select<Series>(s => s.Title.Contains(title));
        }

        public Series GetByTitle(string cleanTitle)
        {
            return Database.Select<Series>(s => s.CleanTitle.Equals(cleanTitle)).SingleOrDefault();
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Database.Select<Series>(s => s.TvDbId.Equals(tvdbId)).SingleOrDefault();
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesType)
        {
            Database.UpdateOnly(new Series { SeriesType = seriesType }, s => s.SeriesType, s => s.Id == seriesId);
        }
    }
}