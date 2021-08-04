using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsRepository
    {
        List<SeasonStatistics> SeriesStatistics();
        List<SeasonStatistics> SeriesStatistics(int seriesId);
    }

    public class SeriesStatisticsRepository : ISeriesStatisticsRepository
    {
        private const string _selectTemplate = "SELECT /**select**/ FROM Episodes /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public SeriesStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<SeasonStatistics> SeriesStatistics()
        {
            var time = DateTime.UtcNow;
            return Query(Builder(time));
        }

        public List<SeasonStatistics> SeriesStatistics(int seriesId)
        {
            var time = DateTime.UtcNow;
            return Query(Builder(time).Where<Episode>(x => x.SeriesId == seriesId));
        }

        private List<SeasonStatistics> Query(SqlBuilder builder)
        {
            var sql = builder.AddTemplate(_selectTemplate).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<SeasonStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder Builder(DateTime currentDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("currentDate", currentDate, null);
            return new SqlBuilder()
            .Select(@"Episodes.SeriesId AS SeriesId,
                             Episodes.SeasonNumber,
                             SUM(COALESCE(EpisodeFiles.Size, 0)) AS SizeOnDisk,
                             GROUP_CONCAT(EpisodeFiles.ReleaseGroup, '|') AS ReleaseGroupsString,
                             COUNT(*) AS TotalEpisodeCount,
                             SUM(CASE WHEN AirdateUtc <= @currentDate OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS AvailableEpisodeCount,
                             SUM(CASE WHEN (Monitored = 1 AND AirdateUtc <= @currentDate) OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeCount,
                             SUM(CASE WHEN EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeFileCount,
                             MIN(CASE WHEN AirDateUtc < @currentDate OR EpisodeFileId > 0 OR Monitored = 0 THEN NULL ELSE AirDateUtc END) AS NextAiringString,
                             MAX(CASE WHEN AirDateUtc >= @currentDate OR EpisodeFileId = 0 AND Monitored = 0 THEN NULL ELSE AirDateUtc END) AS PreviousAiringString", parameters)
            .LeftJoin<Episode, EpisodeFile>((t, f) => t.EpisodeFileId == f.Id)
            .GroupBy<Episode>(x => x.SeriesId)
            .GroupBy<Episode>(x => x.SeasonNumber);
        }
    }
}
