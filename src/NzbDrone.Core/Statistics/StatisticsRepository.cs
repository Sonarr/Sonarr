using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Statistics;

public interface IStatisticsRepository
{
    LibraryStatistics GetLibraryStatistics(StatisticsFilter filter = null);
}

public class StatisticsRepository : IStatisticsRepository
{
    private const string _selectSeriesTemplate = "SELECT /**select**/ FROM \"Series\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
    private const string _selectEpisodesTemplate = "SELECT /**select**/ FROM \"Episodes\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
    private const string _selectEpisodeFilesTemplate = "SELECT /**select**/ FROM \"EpisodeFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
    private const string _selectQualityProfilesTemplate = "SELECT /**select**/ FROM \"QualityProfiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
    private const string _selectTagsTemplate = "SELECT /**select**/ FROM \"Tags\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

    private readonly IMainDatabase _database;

    public StatisticsRepository(IMainDatabase database)
    {
        _database = database;
    }

    public LibraryStatistics GetLibraryStatistics(StatisticsFilter filter = null)
    {
        var currentDate = DateTime.UtcNow;
        var seriesFilter = BuildSeriesFilter(filter);

        var seriesCounts = QuerySingle<SeriesCounts>(SeriesBuilder(seriesFilter), _selectSeriesTemplate);
        var completedSeriesCounts = QueryGrouped<CompletedSeriesCounts>(
            CompletedSeriesBuilder(currentDate, seriesFilter),
            _selectEpisodesTemplate,
            @"COALESCE(SUM(CASE WHEN ""EpisodeCount"" > 0 AND ""EpisodeCount"" = ""EpisodeFileCount"" THEN 1 ELSE 0 END), 0) AS CompletedSeriesCount");
        var seasonCounts = QueryGrouped<SeasonCounts>(
            SeasonsBuilder(currentDate, seriesFilter),
            _selectEpisodesTemplate,
            @"COUNT(*) AS SeasonCount, COALESCE(SUM(CASE WHEN ""EpisodeCount"" > 0 AND ""EpisodeCount"" = ""EpisodeFileCount"" THEN 1 ELSE 0 END), 0) AS CompletedSeasonCount");
        var episodeCounts = QuerySingle<EpisodeCounts>(EpisodesBuilder(currentDate, seriesFilter), _selectEpisodesTemplate);
        var episodeFileCounts = QuerySingle<EpisodeFileCounts>(EpisodeFilesBuilder(seriesFilter), _selectEpisodeFilesTemplate);
        var qualityProfileCounts = Query<QualityProfileCounts>(QualityProfilesBuilder(seriesFilter), _selectQualityProfilesTemplate);
        var qualityProfileFileCounts = Query<QualityProfileFileCounts>(EpisodeFilesPerProfileBuilder(seriesFilter), _selectEpisodeFilesTemplate);
        var qualityCounts = Query<QualityCounts>(EpisodeFilesPerQualityBuilder(seriesFilter), _selectEpisodeFilesTemplate);
        var tagCounts = Query<TagCounts>(TagsBuilder(seriesFilter), _selectTagsTemplate);
        var tagFileCounts = Query<TagFileCounts>(EpisodeFilesPerTagBuilder(seriesFilter), _selectEpisodeFilesTemplate);

        return new LibraryStatistics
        {
            SeriesCount = seriesCounts.SeriesCount,
            MonitoredSeriesCount = seriesCounts.MonitoredSeriesCount,
            CompletedSeriesCount = completedSeriesCounts.CompletedSeriesCount,
            ContinuingSeriesCount = seriesCounts.ContinuingSeriesCount,
            EndedSeriesCount = seriesCounts.EndedSeriesCount,
            UpcomingSeriesCount = seriesCounts.UpcomingSeriesCount,
            DeletedSeriesCount = seriesCounts.DeletedSeriesCount,
            StandardSeriesCount = seriesCounts.StandardSeriesCount,
            DailySeriesCount = seriesCounts.DailySeriesCount,
            AnimeSeriesCount = seriesCounts.AnimeSeriesCount,
            SeasonCount = seasonCounts.SeasonCount,
            CompletedSeasonCount = seasonCounts.CompletedSeasonCount,
            TotalEpisodeCount = episodeCounts.TotalEpisodeCount,
            MonitoredEpisodeCount = episodeCounts.MonitoredEpisodeCount,
            DownloadedEpisodeCount = episodeCounts.DownloadedEpisodeCount,
            MissingEpisodeCount = episodeCounts.MissingEpisodeCount,
            UnairedEpisodeCount = episodeCounts.UnairedEpisodeCount,
            EpisodeFileCount = episodeFileCounts.EpisodeFileCount,
            SizeOnDisk = episodeFileCounts.SizeOnDisk,
            QualityProfileStatistics = MapQualityProfileStatistics(qualityProfileCounts, qualityProfileFileCounts),
            QualityStatistics = MapQualityStatistics(qualityCounts),
            TagStatistics = MapTagStatistics(tagCounts, tagFileCounts)
        };
    }

    private SeriesFilter BuildSeriesFilter(StatisticsFilter filter)
    {
        if (filter == null)
        {
            return null;
        }

        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (filter.RootFolderPaths?.Count > 0)
        {
            var pathConditions = new List<string>();

            for (var i = 0; i < filter.RootFolderPaths.Count; i++)
            {
                // Ensure a trailing separator so '/tv' doesn't match series under '/tv2'
                var path = filter.RootFolderPaths[i];
                var separator = path.Contains('\\') ? '\\' : '/';
                var pathPrefix = path.TrimEnd('/', '\\') + separator;

                // SUBSTR instead of LIKE so characters in the path aren't treated as wildcards
                pathConditions.Add($@"SUBSTR(""Series"".""Path"", 1, @pathPrefixLength{i}) = @pathPrefix{i}");
                parameters.Add($"pathPrefix{i}", pathPrefix, null);
                parameters.Add($"pathPrefixLength{i}", pathPrefix.Length, null);
            }

            conditions.Add(BuildConditionGroup(pathConditions, filter.RootFolderPathsNot));
        }

        if (filter.TagIds?.Count > 0)
        {
            var tagConditions = new List<string>();

            for (var i = 0; i < filter.TagIds.Count; i++)
            {
                tagConditions.Add(SeriesHasTagExpression($"@tagIdFilter{i}"));
                parameters.Add($"tagIdFilter{i}", filter.TagIds[i], null);
            }

            conditions.Add(BuildConditionGroup(tagConditions, filter.TagIdsNot));
        }

        if (filter.QualityProfileIds?.Count > 0)
        {
            var profileConditions = new List<string>();

            for (var i = 0; i < filter.QualityProfileIds.Count; i++)
            {
                profileConditions.Add($@"""Series"".""QualityProfileId"" = @qualityProfileIdFilter{i}");
                parameters.Add($"qualityProfileIdFilter{i}", filter.QualityProfileIds[i], null);
            }

            conditions.Add(BuildConditionGroup(profileConditions, filter.QualityProfileIdsNot));
        }

        if (filter.Monitored.HasValue)
        {
            conditions.Add(@"""Series"".""Monitored"" = @monitoredFilter");
            parameters.Add("monitoredFilter", filter.Monitored.Value, null);
        }

        if (filter.SeriesTypes?.Count > 0)
        {
            var seriesTypeConditions = new List<string>();

            for (var i = 0; i < filter.SeriesTypes.Count; i++)
            {
                seriesTypeConditions.Add($@"""Series"".""SeriesType"" = @seriesTypeFilter{i}");
                parameters.Add($"seriesTypeFilter{i}", (int)filter.SeriesTypes[i], null);
            }

            conditions.Add(BuildConditionGroup(seriesTypeConditions, filter.SeriesTypesNot));
        }

        if (conditions.Count == 0)
        {
            return null;
        }

        return new SeriesFilter
        {
            Condition = string.Join(" AND ", conditions),
            Parameters = parameters
        };
    }

    private static string BuildConditionGroup(List<string> conditions, bool negate)
    {
        var group = $"({string.Join(" OR ", conditions)})";

        return negate ? $"NOT {group}" : group;
    }

    private static List<QualityProfileStatistics> MapQualityProfileStatistics(List<QualityProfileCounts> profileCounts, List<QualityProfileFileCounts> fileCounts)
    {
        var fileCountsByProfile = fileCounts.ToDictionary(f => f.QualityProfileId);

        return profileCounts.Select(p =>
        {
            var files = fileCountsByProfile.GetValueOrDefault(p.QualityProfileId);

            return new QualityProfileStatistics
            {
                QualityProfileId = p.QualityProfileId,
                Name = p.Name,
                SeriesCount = p.SeriesCount,
                EpisodeFileCount = files?.EpisodeFileCount ?? 0,
                SizeOnDisk = files?.SizeOnDisk ?? 0
            };
        }).ToList();
    }

    private static List<TagStatistics> MapTagStatistics(List<TagCounts> tagCounts, List<TagFileCounts> fileCounts)
    {
        var fileCountsByTag = fileCounts.ToDictionary(f => f.TagId);

        return tagCounts.Select(t =>
        {
            var files = fileCountsByTag.GetValueOrDefault(t.TagId);

            return new TagStatistics
            {
                TagId = t.TagId,
                Label = t.Label,
                SeriesCount = t.SeriesCount,
                EpisodeFileCount = files?.EpisodeFileCount ?? 0,
                SizeOnDisk = files?.SizeOnDisk ?? 0
            };
        }).ToList();
    }

    private static List<QualityStatistics> MapQualityStatistics(List<QualityCounts> qualityCounts)
    {
        return qualityCounts.OrderBy(q => q.QualityId)
                            .Select(q => new QualityStatistics
                            {
                                Quality = Quality.FindById(q.QualityId),
                                EpisodeFileCount = q.EpisodeFileCount,
                                SizeOnDisk = q.SizeOnDisk
                            })
                            .ToList();
    }

    private T QuerySingle<T>(SqlBuilder builder, string template)
    {
        return Query<T>(builder, template).Single();
    }

    private List<T> Query<T>(SqlBuilder builder, string template)
    {
        var sql = builder.AddTemplate(template).LogQuery();

        using var conn = _database.OpenConnection();

        return conn.Query<T>(sql.RawSql, sql.Parameters).ToList();
    }

    private T QueryGrouped<T>(SqlBuilder builder, string template, string outerSelect)
    {
        var sql = builder.AddTemplate(template).LogQuery();
        var wrappedSql = $"SELECT {outerSelect} FROM ({sql.RawSql}) AS \"Grouped\"";

        using var conn = _database.OpenConnection();

        return conn.Query<T>(wrappedSql, sql.Parameters).Single();
    }

    // Per-group sums matching the series statistics EpisodeCount semantics
    private SqlBuilder EpisodeProgressBuilder(DateTime currentDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("currentDate", currentDate, null);

        var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";

        return new SqlBuilder(_database.DatabaseType)
            .Select($@"SUM(CASE WHEN (""Episodes"".""Monitored"" = {trueIndicator} AND ""Episodes"".""AirDateUtc"" <= @currentDate) OR ""Episodes"".""EpisodeFileId"" > 0 THEN 1 ELSE 0 END) AS ""EpisodeCount"",
                         SUM(CASE WHEN ""Episodes"".""EpisodeFileId"" > 0 THEN 1 ELSE 0 END) AS ""EpisodeFileCount""",
                parameters);
    }

    private SqlBuilder CompletedSeriesBuilder(DateTime currentDate, SeriesFilter seriesFilter)
    {
        var builder = EpisodeProgressBuilder(currentDate)
            .GroupBy(@"""Episodes"".""SeriesId""");

        if (seriesFilter != null)
        {
            builder.Join<Episode, Series>((e, s) => e.SeriesId == s.Id)
                   .Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder SeasonsBuilder(DateTime currentDate, SeriesFilter seriesFilter)
    {
        var builder = EpisodeProgressBuilder(currentDate)
            .Where(@"""Episodes"".""SeasonNumber"" > 0")
            .GroupBy(@"""Episodes"".""SeriesId"", ""Episodes"".""SeasonNumber""");

        if (seriesFilter != null)
        {
            builder.Join<Episode, Series>((e, s) => e.SeriesId == s.Id)
                   .Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder SeriesBuilder(SeriesFilter seriesFilter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("continuing", (int)SeriesStatusType.Continuing, null);
        parameters.Add("ended", (int)SeriesStatusType.Ended, null);
        parameters.Add("upcoming", (int)SeriesStatusType.Upcoming, null);
        parameters.Add("deleted", (int)SeriesStatusType.Deleted, null);
        parameters.Add("standard", (int)SeriesTypes.Standard, null);
        parameters.Add("daily", (int)SeriesTypes.Daily, null);
        parameters.Add("anime", (int)SeriesTypes.Anime, null);

        var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";

        var builder = new SqlBuilder(_database.DatabaseType)
            .Select($@"COUNT(*) AS SeriesCount,
                         COALESCE(SUM(CASE WHEN ""Monitored"" = {trueIndicator} THEN 1 ELSE 0 END), 0) AS MonitoredSeriesCount,
                         COALESCE(SUM(CASE WHEN ""Status"" = @continuing THEN 1 ELSE 0 END), 0) AS ContinuingSeriesCount,
                         COALESCE(SUM(CASE WHEN ""Status"" = @ended THEN 1 ELSE 0 END), 0) AS EndedSeriesCount,
                         COALESCE(SUM(CASE WHEN ""Status"" = @upcoming THEN 1 ELSE 0 END), 0) AS UpcomingSeriesCount,
                         COALESCE(SUM(CASE WHEN ""Status"" = @deleted THEN 1 ELSE 0 END), 0) AS DeletedSeriesCount,
                         COALESCE(SUM(CASE WHEN ""SeriesType"" = @standard THEN 1 ELSE 0 END), 0) AS StandardSeriesCount,
                         COALESCE(SUM(CASE WHEN ""SeriesType"" = @daily THEN 1 ELSE 0 END), 0) AS DailySeriesCount,
                         COALESCE(SUM(CASE WHEN ""SeriesType"" = @anime THEN 1 ELSE 0 END), 0) AS AnimeSeriesCount",
                parameters);

        if (seriesFilter != null)
        {
            builder.Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder EpisodesBuilder(DateTime currentDate, SeriesFilter seriesFilter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("currentDate", currentDate, null);

        var trueIndicator = _database.DatabaseType == DatabaseType.PostgreSQL ? "true" : "1";

        var builder = new SqlBuilder(_database.DatabaseType)
            .Select($@"COUNT(*) AS TotalEpisodeCount,
                         COALESCE(SUM(CASE WHEN ""Episodes"".""Monitored"" = {trueIndicator} THEN 1 ELSE 0 END), 0) AS MonitoredEpisodeCount,
                         COALESCE(SUM(CASE WHEN ""Episodes"".""EpisodeFileId"" > 0 THEN 1 ELSE 0 END), 0) AS DownloadedEpisodeCount,
                         COALESCE(SUM(CASE WHEN ""Episodes"".""EpisodeFileId"" = 0 AND ""Episodes"".""Monitored"" = {trueIndicator} AND ""Series"".""Monitored"" = {trueIndicator} AND ""Episodes"".""AirDateUtc"" <= @currentDate THEN 1 ELSE 0 END), 0) AS MissingEpisodeCount,
                         COALESCE(SUM(CASE WHEN ""Episodes"".""EpisodeFileId"" = 0 AND (""Episodes"".""AirDateUtc"" IS NULL OR ""Episodes"".""AirDateUtc"" > @currentDate) THEN 1 ELSE 0 END), 0) AS UnairedEpisodeCount",
                parameters)
            .Join<Episode, Series>((e, s) => e.SeriesId == s.Id);

        if (seriesFilter != null)
        {
            builder.Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder EpisodeFilesBuilder(SeriesFilter seriesFilter)
    {
        var builder = new SqlBuilder(_database.DatabaseType)
            .Select(@"COUNT(*) AS EpisodeFileCount,
                        COALESCE(SUM(COALESCE(""EpisodeFiles"".""Size"", 0)), 0) AS SizeOnDisk");

        if (seriesFilter != null)
        {
            builder.Join<EpisodeFile, Series>((f, s) => f.SeriesId == s.Id)
                   .Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder QualityProfilesBuilder(SeriesFilter seriesFilter)
    {
        // Filtering in the join keeps profiles without matching series in the results
        var seriesJoin = @"""Series"" ON ""Series"".""QualityProfileId"" = ""QualityProfiles"".""Id""";

        if (seriesFilter != null)
        {
            seriesJoin += $" AND {seriesFilter.Condition}";
        }

        return new SqlBuilder(_database.DatabaseType)
            .Select(@"""QualityProfiles"".""Id"" AS QualityProfileId,
                        ""QualityProfiles"".""Name"" AS Name,
                        COUNT(""Series"".""Id"") AS SeriesCount")
            .LeftJoin(seriesJoin, seriesFilter?.Parameters)
            .GroupBy(@"""QualityProfiles"".""Id"", ""QualityProfiles"".""Name""")
            .OrderBy(@"""QualityProfiles"".""Name""");
    }

    private SqlBuilder EpisodeFilesPerProfileBuilder(SeriesFilter seriesFilter)
    {
        var builder = new SqlBuilder(_database.DatabaseType)
            .Select(@"""Series"".""QualityProfileId"" AS QualityProfileId,
                        COUNT(*) AS EpisodeFileCount,
                        COALESCE(SUM(COALESCE(""EpisodeFiles"".""Size"", 0)), 0) AS SizeOnDisk")
            .Join<EpisodeFile, Series>((f, s) => f.SeriesId == s.Id)
            .GroupBy(@"""Series"".""QualityProfileId""");

        if (seriesFilter != null)
        {
            builder.Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private string SeriesHasTagExpression(string tagId)
    {
        return _database.DatabaseType == DatabaseType.PostgreSQL
            ? $@"COALESCE(""Series"".""Tags"", '[]')::jsonb @> jsonb_build_array({tagId})"
            : $@"EXISTS (SELECT 1 FROM json_each(COALESCE(""Series"".""Tags"", '[]')) AS ""seriesTag"" WHERE ""seriesTag"".""value"" = {tagId})";
    }

    private SqlBuilder TagsBuilder(SeriesFilter seriesFilter)
    {
        // Filtering in the join keeps tags without matching series in the results
        var seriesJoin = $@"""Series"" ON {SeriesHasTagExpression(@"""Tags"".""Id""")}";

        if (seriesFilter != null)
        {
            seriesJoin += $" AND {seriesFilter.Condition}";
        }

        return new SqlBuilder(_database.DatabaseType)
            .Select(@"""Tags"".""Id"" AS TagId,
                        ""Tags"".""Label"" AS Label,
                        COUNT(""Series"".""Id"") AS SeriesCount")
            .LeftJoin(seriesJoin, seriesFilter?.Parameters)
            .GroupBy(@"""Tags"".""Id"", ""Tags"".""Label""")
            .OrderBy(@"""Tags"".""Label""");
    }

    private SqlBuilder EpisodeFilesPerTagBuilder(SeriesFilter seriesFilter)
    {
        var builder = new SqlBuilder(_database.DatabaseType)
            .Select(@"""Tags"".""Id"" AS TagId,
                        COUNT(*) AS EpisodeFileCount,
                        COALESCE(SUM(COALESCE(""EpisodeFiles"".""Size"", 0)), 0) AS SizeOnDisk")
            .Join<EpisodeFile, Series>((f, s) => f.SeriesId == s.Id)
            .Join($@"""Tags"" ON {SeriesHasTagExpression(@"""Tags"".""Id""")}")
            .GroupBy(@"""Tags"".""Id""");

        if (seriesFilter != null)
        {
            builder.Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private SqlBuilder EpisodeFilesPerQualityBuilder(SeriesFilter seriesFilter)
    {
        var qualityExpression = _database.DatabaseType == DatabaseType.PostgreSQL
            ? @"(""EpisodeFiles"".""Quality""::json->>'quality')::int"
            : @"CAST(JSON_EXTRACT(""EpisodeFiles"".""Quality"", '$.quality') AS INTEGER)";

        var builder = new SqlBuilder(_database.DatabaseType)
            .Select($@"{qualityExpression} AS QualityId,
                        COUNT(*) AS EpisodeFileCount,
                        COALESCE(SUM(COALESCE(""EpisodeFiles"".""Size"", 0)), 0) AS SizeOnDisk")
            .GroupBy(qualityExpression);

        if (seriesFilter != null)
        {
            builder.Join<EpisodeFile, Series>((f, s) => f.SeriesId == s.Id)
                   .Where(seriesFilter.Condition, seriesFilter.Parameters);
        }

        return builder;
    }

    private class SeriesFilter
    {
        public string Condition { get; set; }
        public DynamicParameters Parameters { get; set; }
    }

    private class SeriesCounts
    {
        public int SeriesCount { get; set; }
        public int MonitoredSeriesCount { get; set; }
        public int ContinuingSeriesCount { get; set; }
        public int EndedSeriesCount { get; set; }
        public int UpcomingSeriesCount { get; set; }
        public int DeletedSeriesCount { get; set; }
        public int StandardSeriesCount { get; set; }
        public int DailySeriesCount { get; set; }
        public int AnimeSeriesCount { get; set; }
    }

    private class CompletedSeriesCounts
    {
        public int CompletedSeriesCount { get; set; }
    }

    private class SeasonCounts
    {
        public int SeasonCount { get; set; }
        public int CompletedSeasonCount { get; set; }
    }

    private class EpisodeCounts
    {
        public int TotalEpisodeCount { get; set; }
        public int MonitoredEpisodeCount { get; set; }
        public int DownloadedEpisodeCount { get; set; }
        public int MissingEpisodeCount { get; set; }
        public int UnairedEpisodeCount { get; set; }
    }

    private class EpisodeFileCounts
    {
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
    }

    private class QualityProfileCounts
    {
        public int QualityProfileId { get; set; }
        public string Name { get; set; }
        public int SeriesCount { get; set; }
    }

    private class QualityProfileFileCounts
    {
        public int QualityProfileId { get; set; }
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
    }

    private class QualityCounts
    {
        public int QualityId { get; set; }
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
    }

    private class TagCounts
    {
        public int TagId { get; set; }
        public string Label { get; set; }
        public int SeriesCount { get; set; }
    }

    private class TagFileCounts
    {
        public int TagId { get; set; }
        public int EpisodeFileCount { get; set; }
        public long SizeOnDisk { get; set; }
    }
}
