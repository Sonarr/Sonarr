using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeriesStatistics> SeriesStatistics();
    }

    public class SeriesStatisticsRepository : ISeriesStatisticsRepository
    {
        private readonly IDataMapper _dataMapper;

        public SeriesStatisticsRepository(IDatabase database)
        {
            _dataMapper = database.DataMapper;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            _dataMapper.AddParameter("currentDate", DateTime.UtcNow);

            var queryText = @"SELECT
                              SeriesId,
                              SUM(CASE WHEN Monitored = 1 AND Airdate <= @currentDate THEN 1 ELSE 0 END) AS EpisodeCount,
                              SUM(CASE WHEN Monitored = 1 AND Episodes.EpisodeFileId > 0 AND AirDate <= @currentDate THEN 1 ELSE 0 END) as EpisodeFileCount,
                              MAX(Episodes.SeasonNumber) as SeasonCount,
                              MIN(CASE WHEN AirDate < @currentDate THEN NULL ELSE AirDate END) as NextAiringString
                              FROM Episodes
                              GROUP BY SeriesId";

            return _dataMapper.Query<SeriesStatistics>(queryText);
        }
    }
}
