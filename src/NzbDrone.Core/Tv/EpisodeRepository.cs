using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        Episode Find(int seriesId, int season, int episodeNumber);
        Episode Find(int seriesId, int absoluteEpisodeNumber);
        List<Episode> Find(int seriesId, string date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodesBySeriesIds(List<int> seriesIds);
        List<Episode> GetEpisodesBySceneSeason(int seriesId, int sceneSeasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        List<Episode> EpisodesWithFiles(int seriesId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials, HashSet<int> seriesTags = null);
        PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials, HashSet<int> seriesTags = null, List<int> quality = null);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber);
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials);
        void SetMonitoredFlat(Episode episode, bool monitored);
        void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        List<int> SetMonitored(int seriesId, MonitorTypes monitor, int firstSeason, int lastSeason);
        void SetFileId(Episode episode, int fileId);
        void ClearFileId(Episode episode, bool unmonitor);
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        private readonly Logger _logger;

        public EpisodeRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _logger = logger;
        }

        protected override IEnumerable<Episode> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<Episode, Series>(builder, (episode, series) =>
            {
                episode.Series = series;
                return episode;
            });

        public Episode Find(int seriesId, int season, int episodeNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.SeasonNumber == season && s.EpisodeNumber == episodeNumber)
                               .SingleOrDefault();
        }

        public Episode Find(int seriesId, int absoluteEpisodeNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.AbsoluteEpisodeNumber == absoluteEpisodeNumber)
                        .SingleOrDefault();
        }

        public List<Episode> Find(int seriesId, string date)
        {
            return Query(s => s.SeriesId == seriesId && s.AirDate == date).ToList();
        }

        public List<Episode> GetEpisodes(int seriesId)
        {
            return Query(s => s.SeriesId == seriesId).ToList();
        }

        public List<Episode> GetEpisodes(int seriesId, int seasonNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).ToList();
        }

        public List<Episode> GetEpisodesBySeriesIds(List<int> seriesIds)
        {
            return Query(s => seriesIds.Contains(s.SeriesId)).ToList();
        }

        public List<Episode> GetEpisodesBySceneSeason(int seriesId, int seasonNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.SceneSeasonNumber == seasonNumber).ToList();
        }

        public List<Episode> GetEpisodeByFileId(int fileId)
        {
            return Query(e => e.EpisodeFileId == fileId).ToList();
        }

        public List<Episode> EpisodesWithFiles(int seriesId)
        {
            var builder = Builder()
                .Join<Episode, EpisodeFile>((e, ef) => e.EpisodeFileId == ef.Id)
                .Where<Episode>(e => e.SeriesId == seriesId);

            return _database.QueryJoined<Episode, EpisodeFile>(
                builder,
                (episode, episodeFile) =>
                {
                    episode.EpisodeFile = episodeFile;
                    return episode;
                }).ToList();
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials, HashSet<int> seriesTags = null)
        {
            var currentTime = DateTime.UtcNow;
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWithoutFilesBuilder(currentTime, startingSeasonNumber, seriesTags), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWithoutFilesBuilder(currentTime, startingSeasonNumber, seriesTags).SelectCountDistinct<Episode>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials, HashSet<int> seriesTags = null, List<int> quality = null)
        {
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingSeasonNumber, seriesTags, quality), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Episode))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, startingSeasonNumber, seriesTags, quality).Select(typeof(Episode)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.SceneSeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber).ToList();
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber)
        {
            return Query(s => s.SeriesId == seriesId && s.SceneAbsoluteEpisodeNumber == sceneAbsoluteEpisodeNumber).ToList();
        }

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored, bool includeSpecials)
        {
            var builder = Builder().Where<Episode>(rg => rg.AirDateUtc >= startDate && rg.AirDateUtc <= endDate);

            if (!includeSpecials)
            {
                builder = builder.Where<Episode>(e => e.SeasonNumber != 0);
            }

            if (!includeUnmonitored)
            {
                builder = builder.Where<Episode>(e => e.Monitored == true)
                    .Join<Episode, Series>((l, r) => l.SeriesId == r.Id)
                    .Where<Series>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Episode episode, bool monitored)
        {
            episode.Monitored = monitored;
            SetFields(episode, p => p.Monitored);

            ModelUpdated(episode, true);
        }

        public void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored)
        {
            using (var conn = _database.OpenConnection())
            {
                conn.Execute("UPDATE \"Episodes\" SET \"Monitored\" = @monitored WHERE \"SeriesId\" = @seriesId AND \"SeasonNumber\" = @seasonNumber AND \"Monitored\" != @monitored",
                    new { seriesId = seriesId, seasonNumber = seasonNumber, monitored = monitored });
            }
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var episodes = ids.Select(x => new Episode { Id = x, Monitored = monitored }).ToList();
            SetFields(episodes, p => p.Monitored);
        }

        public List<int> SetMonitored(int seriesId, MonitorTypes monitor, int firstSeason, int lastSeason)
        {
            var parameters = new DynamicParameters();
            parameters.Add("seriesId", seriesId);

            using var conn = _database.OpenConnection();

            // Specials are toggled on their own and the regular seasons are left untouched.
            if (monitor is MonitorTypes.MonitorSpecials or MonitorTypes.UnmonitorSpecials)
            {
                SetMonitoredWhere(conn, null, "\"SeriesId\" = @seriesId AND \"SeasonNumber\" = 0", monitor == MonitorTypes.MonitorSpecials, parameters);

                return GetSeasonNumbersWithMonitoredEpisodes(conn, seriesId);
            }

            if (monitor == MonitorTypes.None)
            {
                SetMonitoredWhere(conn, null, "\"SeriesId\" = @seriesId", false, parameters);

                return new List<int>();
            }

            // The predicate selects the episodes to monitor; everything else in the series is unmonitored.
            string predicate;

            if (monitor == MonitorTypes.All)
            {
                predicate = "\"SeasonNumber\" > 0";
            }
            else if (monitor == MonitorTypes.Future)
            {
                predicate = "\"SeasonNumber\" > 0 AND (\"AirDateUtc\" IS NULL OR \"AirDateUtc\" >= @now)";
                parameters.Add("now", DateTime.UtcNow);
            }
            else if (monitor == MonitorTypes.Missing)
            {
                predicate = "\"SeasonNumber\" > 0 AND \"EpisodeFileId\" = 0";
            }
            else if (monitor == MonitorTypes.Existing)
            {
                predicate = "\"SeasonNumber\" > 0 AND \"EpisodeFileId\" <> 0";
            }
            else if (monitor == MonitorTypes.Pilot)
            {
                predicate = "\"SeasonNumber\" > 0 AND \"SeasonNumber\" = @firstSeason AND \"EpisodeNumber\" = 1";
                parameters.Add("firstSeason", firstSeason);
            }
            else if (monitor == MonitorTypes.FirstSeason)
            {
                predicate = "\"SeasonNumber\" > 0 AND \"SeasonNumber\" = @firstSeason";
                parameters.Add("firstSeason", firstSeason);
            }
#pragma warning disable CS0612
            else if (monitor is MonitorTypes.LastSeason or MonitorTypes.LatestSeason)
#pragma warning restore CS0612
            {
                predicate = "\"SeasonNumber\" > 0 AND \"SeasonNumber\" = @lastSeason";
                parameters.Add("lastSeason", lastSeason);
            }
            else if (monitor == MonitorTypes.Recent)
            {
                predicate = "\"SeasonNumber\" > 0 AND (\"AirDateUtc\" IS NULL OR \"AirDateUtc\" >= @cutoff)";
                parameters.Add("cutoff", DateTime.UtcNow.AddDays(-90));
            }
            else
            {
                return GetSeasonNumbersWithMonitoredEpisodes(conn, seriesId);
            }

            using (var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                SetMonitoredWhere(conn, tran, $"\"SeriesId\" = @seriesId AND ({predicate})", true, parameters);
                SetMonitoredWhere(conn, tran, $"\"SeriesId\" = @seriesId AND NOT ({predicate})", false, parameters);
                tran.Commit();
            }

            return GetSeasonNumbersWithMonitoredEpisodes(conn, seriesId);
        }

        public void SetFileId(Episode episode, int fileId)
        {
            episode.EpisodeFileId = fileId;

            SetFields(episode, ep => ep.EpisodeFileId);

            ModelUpdated(episode, true);
        }

        public void ClearFileId(Episode episode, bool unmonitor)
        {
            episode.EpisodeFileId = 0;
            episode.Monitored &= !unmonitor;

            SetFields(episode, ep => ep.EpisodeFileId, ep => ep.Monitored);

            ModelUpdated(episode, true);
        }

        private SqlBuilder EpisodesWithoutFilesBuilder(DateTime currentTime, int startingSeasonNumber, HashSet<int> seriesTags)
        {
            var builder = Builder()
                .Join<Episode, Series>((l, r) => l.SeriesId == r.Id)
                .Where<Episode>(f => f.EpisodeFileId == 0)
                .Where<Episode>(f => f.SeasonNumber >= startingSeasonNumber)
                .Where(BuildAirDateUtcCutoffWhereClause(currentTime));

            if (seriesTags is { Count: > 0 })
            {
                builder = builder.Where(BuildSeriesTagsWhereClause(seriesTags));
            }

            return builder;
        }

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                return string.Format("\"Episodes\".\"AirDateUtc\" + make_interval(mins => \"Series\".\"Runtime\") <= '{0}'",
                                     currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return string.Format("datetime(strftime('%s', \"Episodes\".\"AirDateUtc\") + \"Series\".\"Runtime\" * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private SqlBuilder EpisodesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff, int startingSeasonNumber, HashSet<int> seriesTags, List<int> qualities)
        {
            var builder = Builder()
                .Join<Episode, Series>((e, s) => e.SeriesId == s.Id)
                .LeftJoin<Episode, EpisodeFile>((e, ef) => e.EpisodeFileId == ef.Id)
                .Where<Episode>(e => e.EpisodeFileId != 0)
                .Where<Episode>(e => e.SeasonNumber >= startingSeasonNumber)
                .Where(
                    string.Format("({0})",
                        BuildQualityCutoffWhereClause(qualitiesBelowCutoff)));

            if (seriesTags is { Count: > 0 })
            {
                builder = builder.Where(BuildSeriesTagsWhereClause(seriesTags));
            }

            if (qualities is { Count: > 0 })
            {
                builder = builder.Where(BuildQualityFilterWhereClause(qualities));
            }

            return builder
                .GroupBy<Episode>(e => e.Id)
                .GroupBy<Series>(s => s.Id);
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(\"Series\".\"QualityProfileId\" = {0} AND \"EpisodeFiles\".\"Quality\" LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private string BuildSeriesTagsWhereClause(HashSet<int> tagIds)
        {
            var ids = string.Join(",", tagIds);

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                return string.Format(
                    "EXISTS (SELECT 1 FROM jsonb_array_elements_text(\"Series\".\"Tags\"::jsonb) AS elem WHERE elem::int IN ({0}))",
                    ids);
            }

            return string.Format(
                "EXISTS (SELECT 1 FROM json_each(\"Series\".\"Tags\") WHERE json_each.value IN ({0}))",
                ids);
        }

        private string BuildQualityFilterWhereClause(List<int> qualityIds)
        {
            var clauses = qualityIds
                .Select(id => string.Format("\"EpisodeFiles\".\"Quality\" LIKE '%_quality_: {0},%'", id))
                .ToList();

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private List<int> GetSeasonNumbersWithMonitoredEpisodes(IDbConnection conn, int seriesId)
        {
            return conn.Query<int>("SELECT DISTINCT \"SeasonNumber\" FROM \"Episodes\" WHERE \"SeriesId\" = @seriesId AND \"Monitored\" = @monitored",
                new { seriesId, monitored = true }).ToList();
        }

        private void SetMonitoredWhere(IDbConnection conn, IDbTransaction tran, string whereClause, bool monitored, DynamicParameters parameters)
        {
            var p = new DynamicParameters(parameters);
            p.Add("monitored", monitored);

            // Only rows that actually change are written.
            conn.Execute($"UPDATE \"Episodes\" SET \"Monitored\" = @monitored WHERE {whereClause} AND \"Monitored\" <> @monitored", p, tran);
        }

        private Episode FindOneByAirDate(int seriesId, string date)
        {
            var episodes = Query(s => s.SeriesId == seriesId && s.AirDate == date).ToList();

            if (!episodes.Any())
            {
                return null;
            }

            if (episodes.Count == 1)
            {
                return episodes.First();
            }

            _logger.Debug("Multiple episodes with the same air date were found, will exclude specials");

            var regularEpisodes = episodes.Where(e => e.SeasonNumber > 0).ToList();

            if (regularEpisodes.Count == 1)
            {
                _logger.Debug("Left with one episode after excluding specials");
                return regularEpisodes.First();
            }

            throw new InvalidOperationException("Multiple episodes with the same air date found");
        }
    }
}
