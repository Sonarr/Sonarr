using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeriesStatistics> SeriesStatistics();
        SeriesStatistics SeriesStatistics(int seriesId);
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

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<SeriesStatistics>(queryText);
        }

        public SeriesStatistics SeriesStatistics(int seriesId)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);
            mapper.AddParameter("seriesId", seriesId);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine("WHERE SeriesId = @seriesId");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Find<SeriesStatistics>(queryText);
        }

        private string GetSelectClause()
        {
            return @"SELECT
                     SeriesId,
                     SUM(CASE WHEN Monitored = 1 AND AirdateUtc <= @currentDate THEN 1 ELSE 0 END) AS EpisodeCount,
                     SUM(CASE WHEN Monitored = 1 AND Episodes.EpisodeFileId > 0 AND AirDateUtc <= @currentDate THEN 1 ELSE 0 END) AS EpisodeFileCount,
                     MAX(Episodes.SeasonNumber) as SeasonCount,
                     MIN(CASE WHEN AirDateUtc < @currentDate THEN NULL ELSE AirDate END) AS NextAiringString
                     FROM Episodes";
        }

        private string GetGroupByClause()
        {
            return "GROUP BY SeriesId";
        }
    }
}
