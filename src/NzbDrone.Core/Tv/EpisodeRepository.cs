using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Tv
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        Episode Find(int seriesId, int season, int episodeNumber);
        Episode Find(int seriesId, int absoluteEpisodeNumber);
        Episode Get(int seriesId, string date);
        Episode Find(int seriesId, string date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        List<Episode> EpisodesWithFiles(int seriesId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials);
        PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber);
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Episode episode, bool monitored);
        void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        void SetFileId(int episodeId, int fileId);
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public EpisodeRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _database = database;
            _logger = logger;
        }

        public Episode Find(int seriesId, int season, int episodeNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId)
                               .AndWhere(s => s.SeasonNumber == season)
                               .AndWhere(s => s.EpisodeNumber == episodeNumber)
                               .SingleOrDefault();
        }

        public Episode Find(int seriesId, int absoluteEpisodeNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId)
                        .AndWhere(s => s.AbsoluteEpisodeNumber == absoluteEpisodeNumber)
                        .SingleOrDefault();
        }

        public Episode Get(int seriesId, string date)
        {
            var episode = FindOneByAirDate(seriesId, date);

            if (episode == null)
            {
                throw new InvalidOperationException("Expected at one episode");
            }

            return episode;
        }

        public Episode Find(int seriesId, string date)
        {
            return FindOneByAirDate(seriesId, date);
        }

        public List<Episode> GetEpisodes(int seriesId)
        {
            return Query.Where(s => s.SeriesId == seriesId).ToList();
        }

        public List<Episode> GetEpisodes(int seriesId, int seasonNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId)
                        .AndWhere(s => s.SeasonNumber == seasonNumber)
                        .ToList();
        }

        public List<Episode> GetEpisodeByFileId(int fileId)
        {
            return Query.Where(e => e.EpisodeFileId == fileId).ToList();
        }

        public List<Episode> EpisodesWithFiles(int seriesId)
        {
            return Query.Join<Episode, EpisodeFile>(JoinType.Inner, e => e.EpisodeFile, (e, ef) => e.EpisodeFileId == ef.Id)
                        .Where(e => e.SeriesId == seriesId);
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.TotalRecords = GetMissingEpisodesQuery(pagingSpec, currentTime, startingSeasonNumber).GetRowCount();
            pagingSpec.Records = GetMissingEpisodesQuery(pagingSpec, currentTime, startingSeasonNumber).ToList();

            return pagingSpec;
        }

        public PagingSpec<Episode> EpisodesWhereCutoffUnmet(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, bool includeSpecials)
        {
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.TotalRecords = EpisodesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff, startingSeasonNumber).GetRowCount();
            pagingSpec.Records = EpisodesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff, startingSeasonNumber).ToList();

            return pagingSpec;
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId)
                        .AndWhere(s => s.SceneSeasonNumber == seasonNumber)
                        .AndWhere(s => s.SceneEpisodeNumber == episodeNumber)
                        .ToList();
        }

        public List<Episode> FindEpisodesBySceneNumbering(int seriesId, int sceneAbsoluteEpisodeNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId)
                        .AndWhere(s => s.SceneAbsoluteEpisodeNumber == sceneAbsoluteEpisodeNumber)
                        .ToList();
        }

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var query = Query.Join<Episode, Series>(JoinType.Inner, e => e.Series, (e, s) => e.SeriesId == s.Id)
                             .Where<Episode>(e => e.AirDateUtc >= startDate)
                             .AndWhere(e => e.AirDateUtc <= endDate);


            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored)
                     .AndWhere(e => e.Series.Monitored);
            }

            return query.ToList();
        }

        public void SetMonitoredFlat(Episode episode, bool monitored)
        {
            episode.Monitored = monitored;
            SetFields(episode, p => p.Monitored);
        }

        public void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("seriesId", seriesId);
            mapper.AddParameter("seasonNumber", seasonNumber);
            mapper.AddParameter("monitored", monitored);

            const string sql = "UPDATE Episodes " +
                               "SET Monitored = @monitored " +
                               "WHERE SeriesId = @seriesId " +
                               "AND SeasonNumber = @seasonNumber";

            mapper.ExecuteNonQuery(sql);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("monitored", monitored);

            var sql = "UPDATE Episodes " +
                      "SET Monitored = @monitored " +
                      $"WHERE Id IN ({string.Join(", ", ids)})";

            mapper.ExecuteNonQuery(sql);
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Episode { Id = episodeId, EpisodeFileId = fileId }, episode => episode.EpisodeFileId);
        }

        private SortBuilder<Episode> GetMissingEpisodesQuery(PagingSpec<Episode> pagingSpec, DateTime currentTime, int startingSeasonNumber)
        {
            return Query.Join<Episode, Series>(JoinType.Inner, e => e.Series, (e, s) => e.SeriesId == s.Id)
                            .Where(pagingSpec.FilterExpressions.FirstOrDefault())
                            .AndWhere(e => e.EpisodeFileId == 0)
                            .AndWhere(e => e.SeasonNumber >= startingSeasonNumber)
                            .AndWhere(BuildAirDateUtcCutoffWhereClause(currentTime))
                            .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                            .Skip(pagingSpec.PagingOffset())
                            .Take(pagingSpec.PageSize);
        }

        private SortBuilder<Episode> EpisodesWhereCutoffUnmetQuery(PagingSpec<Episode> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff, int startingSeasonNumber)
        {
            return Query.Join<Episode, Series>(JoinType.Inner, e => e.Series, (e, s) => e.SeriesId == s.Id)
                             .Join<Episode, EpisodeFile>(JoinType.Left, e => e.EpisodeFile, (e, s) => e.EpisodeFileId == s.Id)
                             .Where(pagingSpec.FilterExpressions.FirstOrDefault())
                             .AndWhere(e => e.EpisodeFileId != 0)
                             .AndWhere(e => e.SeasonNumber >= startingSeasonNumber)
                             .AndWhere(BuildQualityCutoffWhereClause(qualitiesBelowCutoff))
                             .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                             .Skip(pagingSpec.PagingOffset())
                             .Take(pagingSpec.PageSize);
        }

        private string BuildAirDateUtcCutoffWhereClause(DateTime currentTime)
        {
            return string.Format("WHERE datetime(strftime('%s', [t0].[AirDateUtc]) + [t1].[RunTime] * 60,  'unixepoch') <= '{0}'",
                                 currentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("([t1].[ProfileId] = {0} AND [t2].[Quality] LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private Episode FindOneByAirDate(int seriesId, string date)
        {
            var episodes = Query.Where(s => s.SeriesId == seriesId)
                                .AndWhere(s => s.AirDate == date)
                                .ToList();

            if (!episodes.Any()) return null;

            if (episodes.Count == 1) return episodes.First();

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
