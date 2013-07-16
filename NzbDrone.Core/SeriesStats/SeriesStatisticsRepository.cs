using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeriesStatistics> SeriesStatistics();
    }

    public class SeriesStatisticsRepository : ISeriesStatisticsRepository
    {
        private readonly IDatabase _database;

        public SeriesStatisticsRepository(IDatabase database)
        {
            _database = database;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);

            const string queryText = @"SELECT
                              SeriesId,
                              SUM(CASE WHEN Monitored = 1 AND Airdate <= @currentDate THEN 1 ELSE 0 END) AS EpisodeCount,
                              SUM(CASE WHEN Monitored = 1 AND Episodes.EpisodeFileId > 0 AND AirDate <= @currentDate THEN 1 ELSE 0 END) as EpisodeFileCount,
                              MAX(Episodes.SeasonNumber) as SeasonCount,
                              MIN(CASE WHEN AirDate < @currentDate THEN NULL ELSE AirDate END) as NextAiringString
                              FROM Episodes
                              GROUP BY SeriesId";

            return mapper.Query<SeriesStatistics>(queryText);
        }
    }
}
