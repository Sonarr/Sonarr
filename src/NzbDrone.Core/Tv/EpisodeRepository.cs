using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        Episode Find(int seriesId, int season, int episodeNumber);
        Episode Find(int seriesId, int absoluteEpisodeNumber);
        Episode Get(int seriesId, String date);
        Episode Find(int seriesId, String date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials);
        Episode FindEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate);
        void SetMonitoredFlat(Episode episode, bool monitored);
        void SetMonitoredBySeason(int seriesId, int seasonNumber, bool monitored);
        void SetFileId(int episodeId, int fileId);
        IEnumerable<Episode> SearchForEpisodes(string episodeTitle, int seriesId);
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        private readonly IDatabase _database;

        public EpisodeRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public Episode Find(int seriesId, int season, int episodeNumber)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SeasonNumber == season && s.EpisodeNumber == episodeNumber);
        }

        public Episode Find(int seriesId, int absoluteEpisodeNumber)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.AbsoluteEpisodeNumber == absoluteEpisodeNumber);
        }

        public Episode Get(int seriesId, String date)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.AirDate == date);
        }

        public Episode Find(int seriesId, String date)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.AirDate == date);
        }

        public List<Episode> GetEpisodes(int seriesId)
        {
            return Query.Where(s => s.SeriesId == seriesId).ToList();
        }

        public List<Episode> GetEpisodes(int seriesId, int seasonNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).ToList();
        }

        public List<Episode> GetEpisodeByFileId(int fileId)
        {
            return Query.Where(e => e.EpisodeFileId == fileId).ToList();
        }

        private static readonly Regex NormalizeSpacesRegex = new Regex(@"\W+", RegexOptions.Compiled);
        private static readonly Regex NormalizeWordsRegex = new Regex(@"\b(a|an|the|and|or|of|part)\b\s?",
                                                         RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static string NormalizeTitle(string title)
        {
            // convert any space-like characters to a single space
            string normalizedSpaces = NormalizeSpacesRegex.Replace(title, " ").ToLower();
            // remove common words
            string normalized = NormalizeWordsRegex.Replace(normalizedSpaces, String.Empty);
            return normalized;
        }

        public IEnumerable<Episode> SearchForEpisodes(string episodeTitle, int seriesId)
        {
            var search = NormalizeTitle(episodeTitle);
            var episodes = GetEpisodes(seriesId);
            return episodes.Where(
                e => 
                {
                    // TODO: can replace this search mechanism with something smarter/faster/better
                    // normalize episode title
                    string title = NormalizeTitle(e.Title);
                    // find episode title within search string
                    return (title.Length > 0) && search.Contains(title); 
                });
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            pagingSpec.Records = GetEpisodesWithoutFilesQuery(pagingSpec, currentTime, startingSeasonNumber).ToList();
            pagingSpec.TotalRecords = GetEpisodesWithoutFilesQuery(pagingSpec, currentTime, startingSeasonNumber).GetRowCount();

            return pagingSpec;
        }

        public Episode FindEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SceneSeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber);
        }


        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate)
        {
            return Query.Join<Episode, Series>(JoinType.Inner, e => e.Series, (e, s) => e.SeriesId == s.Id)
                        .Where<Episode>(e => e.AirDateUtc >= startDate)
                        .AndWhere(e => e.AirDateUtc <= endDate)
                        .AndWhere(e => e.Monitored)
                        .AndWhere(e => e.Series.Monitored)
                        .ToList();
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

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Episode { Id = episodeId, EpisodeFileId = fileId }, episode => episode.EpisodeFileId);
        }

        private SortBuilder<Episode> GetEpisodesWithoutFilesQuery(PagingSpec<Episode> pagingSpec, DateTime currentTime, int startingSeasonNumber)
        {
            return Query.Join<Episode, Series>(JoinType.Inner, e => e.Series, (e, s) => e.SeriesId == s.Id)
                        .Where(e => e.EpisodeFileId == 0)
                        .AndWhere(e => e.SeasonNumber >= startingSeasonNumber)
                        .AndWhere(e => e.AirDateUtc <= currentTime)
                        .AndWhere(e => e.Monitored)
                        .AndWhere(e => e.Series.Monitored)
                        .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                        .Skip(pagingSpec.PagingOffset())
                        .Take(pagingSpec.PageSize);
        }
    }
}
