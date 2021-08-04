using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
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
        List<Episode> GetEpisodesBySceneSeason(int seriesId, int sceneSeasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        List<Episode> EpisodesWithFiles(int seriesId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials);
        PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff, bool includeSpecials);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber);
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Episode episode, bool monitored);
        void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
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

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWithoutFilesBuilder(currentTime, startingSeasonNumber), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWithoutFilesBuilder(currentTime, startingSeasonNumber).SelectCountDistinct<Episode>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff, bool includeSpecials)
        {
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.Records = GetPagedRecords(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, languagesBelowCutoff, startingSeasonNumber), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM {TableMapping.Mapper.TableNameMapping(typeof(Episode))} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/)";
            pagingSpec.TotalRecords = GetPagedRecordCount(EpisodesWhereCutoffUnmetBuilder(qualitiesBelowCutoff, languagesBelowCutoff, startingSeasonNumber).Select(typeof(Episode)), pagingSpec, countTemplate);

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

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Episode>(rg => rg.AirDateUtc >= startDate && rg.AirDateUtc <= endDate);

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
                conn.Execute("UPDATE Episodes SET Monitored = @monitored WHERE SeriesId = @seriesId AND SeasonNumber = @seasonNumber AND Monitored != @monitored",
                    new { seriesId = seriesId, seasonNumber = seasonNumber, monitored = monitored });
            }
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var episodes = ids.Select(x => new Episode { Id = x, Monitored = monitored }).ToList();
            SetFields(episodes, p => p.Monitored);
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

        private SqlBuilder EpisodesWithoutFilesBuilder(DateTime currentTime, int startingSeasonNumber) => Builder()
            .Join<Episode, Series>((l, r) => l.SeriesId == r.Id)
            .Where<Episode>(f => f.EpisodeFileId == 0)
            .Where<Episode>(f => f.SeasonNumber >= startingSeasonNumber)
            .Where(BuildAirDateUtcCutoffWhereClause(currentTime));

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            return string.Format("datetime(strftime('%s', \"Episodes\".\"AirDateUtc\") + \"Series\".\"RunTime\" * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private SqlBuilder EpisodesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff, List<LanguagesBelowCutoff> languagesBelowCutoff, int startingSeasonNumber) => Builder()
            .Join<Episode, Series>((e, s) => e.SeriesId == s.Id)
            .LeftJoin<Episode, EpisodeFile>((e, ef) => e.EpisodeFileId == ef.Id)
            .Where<Episode>(e => e.EpisodeFileId != 0)
            .Where<Episode>(e => e.SeasonNumber >= startingSeasonNumber)
            .Where(
                string.Format("({0} OR {1})",
                    BuildQualityCutoffWhereClause(qualitiesBelowCutoff),
                    BuildLanguageCutoffWhereClause(languagesBelowCutoff)))
            .GroupBy<Episode>(e => e.Id);

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(Series.[QualityProfileId] = {0} AND EpisodeFiles.Quality LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private string BuildLanguageCutoffWhereClause(List<LanguagesBelowCutoff> languagesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in languagesBelowCutoff)
            {
                foreach (var belowCutoff in profile.LanguageIds)
                {
                    clauses.Add(string.Format("(Series.[LanguageProfileId] = {0} AND EpisodeFiles.Language = {1})", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
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
