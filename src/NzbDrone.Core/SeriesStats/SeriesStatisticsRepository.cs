using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string _selectEpisodesTemplate = "SELECT /**select**/ FROM Episodes /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectEpisodeFilesTemplate = "SELECT /**select**/ FROM EpisodeFiles /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public SeriesStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<SeasonStatistics> SeriesStatistics()
        {
            var time = DateTime.UtcNow;
            return MapResults(Query(EpisodesBuilder(time), _selectEpisodesTemplate),
                Query(EpisodeFilesBuilder(), _selectEpisodeFilesTemplate));
        }

        public List<SeasonStatistics> SeriesStatistics(int seriesId)
        {
            var time = DateTime.UtcNow;

            return MapResults(Query(EpisodesBuilder(time).Where<Episode>(x => x.SeriesId == seriesId), _selectEpisodesTemplate),
                Query(EpisodeFilesBuilder().Where<EpisodeFile>(x => x.SeriesId == seriesId), _selectEpisodeFilesTemplate));
        }

        private List<SeasonStatistics> MapResults(List<SeasonStatistics> episodesResult, List<SeasonStatistics> filesResult)
        {
            episodesResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.SeriesId == e.SeriesId & f.SeasonNumber == e.SeasonNumber);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return episodesResult;
        }

        private List<SeasonStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<SeasonStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder EpisodesBuilder(DateTime currentDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("currentDate", currentDate, null);

            return new SqlBuilder()
            .Select(@"Episodes.SeriesId AS SeriesId,
                             Episodes.SeasonNumber,
                             COUNT(*) AS TotalEpisodeCount,
                             SUM(CASE WHEN AirdateUtc <= @currentDate OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS AvailableEpisodeCount,
                             SUM(CASE WHEN (Monitored = 1 AND AirdateUtc <= @currentDate) OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeCount,
                             SUM(CASE WHEN EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeFileCount,
                             MIN(CASE WHEN AirDateUtc < @currentDate OR EpisodeFileId > 0 OR Monitored = 0 THEN NULL ELSE AirDateUtc END) AS NextAiringString,
                             MAX(CASE WHEN AirDateUtc >= @currentDate OR EpisodeFileId = 0 AND Monitored = 0 THEN NULL ELSE AirDateUtc END) AS PreviousAiringString", parameters)
            .GroupBy<Episode>(x => x.SeriesId)
            .GroupBy<Episode>(x => x.SeasonNumber);
        }

        private SqlBuilder EpisodeFilesBuilder()
        {
            return new SqlBuilder()
                .Select(@"SeriesId,
                            SeasonNumber,
                            SUM(COALESCE(Size, 0)) AS SizeOnDisk,
                            GROUP_CONCAT(ReleaseGroup, '|') AS ReleaseGroupsString")
                .GroupBy<EpisodeFile>(x => x.SeriesId)
                .GroupBy<EpisodeFile>(x => x.SeasonNumber);
        }
    }
}
