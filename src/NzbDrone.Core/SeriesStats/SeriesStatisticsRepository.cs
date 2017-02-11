using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeasonStatistics> SeriesStatistics();
        List<SeasonStatistics> SeriesStatistics(int seriesId);
    }

    public class SeriesStatisticsRepository : ISeriesStatisticsRepository
    {
        private readonly IMainDatabase _database;

        public SeriesStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<SeasonStatistics> SeriesStatistics()
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine(GetEpisodeFilesJoin());
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<SeasonStatistics>(queryText);
        }

        public List<SeasonStatistics> SeriesStatistics(int seriesId)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);
            mapper.AddParameter("seriesId", seriesId);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine(GetEpisodeFilesJoin());
            sb.AppendLine("WHERE Episodes.SeriesId = @seriesId");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<SeasonStatistics>(queryText);
        }

        private string GetSelectClause()
        {
            return @"SELECT Episodes.*, SUM(EpisodeFiles.Size) as SizeOnDisk FROM
                     (SELECT
                     Episodes.SeriesId,
                     Episodes.SeasonNumber,
                     COUNT(*) AS TotalEpisodeCount,
                     SUM(CASE WHEN AirdateUtc <= @currentDate OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS AvailableEpisodeCount,
                     SUM(CASE WHEN (Monitored = 1 AND AirdateUtc <= @currentDate) OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeCount,
                     SUM(CASE WHEN EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeFileCount,
                     MIN(CASE WHEN AirDateUtc < @currentDate OR EpisodeFileId > 0 OR Monitored = 0 THEN NULL ELSE AirDateUtc END) AS NextAiringString,
                     MAX(CASE WHEN AirDateUtc >= @currentDate OR EpisodeFileId = 0 AND Monitored = 0 THEN NULL ELSE AirDateUtc END) AS PreviousAiringString
                     FROM Episodes
                     GROUP BY Episodes.SeriesId, Episodes.SeasonNumber) as Episodes";
        }

        private string GetGroupByClause()
        {
            return "GROUP BY Episodes.SeriesId, Episodes.SeasonNumber";
        }

        private string GetEpisodeFilesJoin()
        {
            return @"LEFT OUTER JOIN EpisodeFiles
                     ON EpisodeFiles.SeriesId = Episodes.SeriesId
                     AND EpisodeFiles.SeasonNumber = Episodes.SeasonNumber";
        }
    }
}
