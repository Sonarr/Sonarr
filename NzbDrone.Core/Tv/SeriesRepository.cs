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
        void SetTvRageId(int seriesId, int tvRageId);
        List<SeriesStatistics> SeriesStatistics();
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        private readonly IDatabase _database;

        public SeriesRepository(IDatabase database)
            : base(database)
        {
            _database = database;
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

        public void SetTvRageId(int seriesId, int tvRageId)
        {
            SetFields(new Series { Id = seriesId, TvRageId = tvRageId }, s => s.TvRageId);
        }
        
        public List<SeriesStatistics> SeriesStatistics()
        {
            _database.DataMapper.AddParameter("currentDate", DateTime.UtcNow);

            var queryText = @"SELECT
                              SeriesId,
                              SUM(CASE WHEN Airdate <= @currentDate THEN 1 ELSE 0 END) AS EpisodeCount,
                              SUM(CASE WHEN EpisodeFileId > 0 AND AirDate <= @currentDate THEN 1 ELSE 0 END) as EpisodeFileCount,
                              MAX(SeasonNumber) as NumberOfSeasons,
                              MIN(CASE WHEN AirDate < @currentDate THEN NULL ELSE AirDate END) as NextAiring
                              FROM Episodes
                              WHERE Ignored = 0
                              GROUP BY SeriesId";

            return _database.DataMapper.Query<SeriesStatistics>(queryText);
        }
    }
}