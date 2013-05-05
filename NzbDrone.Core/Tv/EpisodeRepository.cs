using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Marr.Data;
using Marr.Data.QGen;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Model;


namespace NzbDrone.Core.Tv
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        Episode Get(int seriesId, int season, int episodeNumber);
        Episode Get(int seriesId, DateTime date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials);
        Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> EpisodesWithFiles();
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate);
        void SetIgnoreFlat(Episode episode, bool ignoreFlag);
        void SetPostDownloadStatus(int episodeId, PostDownloadStatusType status);
        void SetFileId(int episodeId, int fileId);
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        private readonly IDataMapper _dataMapper;

        public EpisodeRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
        {
            _dataMapper = database.DataMapper;
        }

        public Episode Get(int seriesId, int season, int episodeNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == season && s.EpisodeNumber == episodeNumber);
        }

        public Episode Get(int seriesId, DateTime date)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.AirDate.HasValue && s.AirDate.Value.Date == date.Date);
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
            return Query.Where(s => s.EpisodeFile != null && s.EpisodeFile.Id == fileId).ToList();
        }

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials)
        {
            //TODO: Join in the series title so we can do sorting on it
            if (!pagingSpec.SortKey.Equals("SeriesTitle", StringComparison.InvariantCultureIgnoreCase) &&
                !pagingSpec.SortKey.Equals("AirDate", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Invalid SortKey: " + pagingSpec.SortKey, "pagingSpec");
            }

            if (includeSpecials)
            {
                throw new NotImplementedException("Including specials is not available");
            }

            var orderSql = String.Format("{0} {1}", pagingSpec.SortKey,
                                         pagingSpec.SortDirection == ListSortDirection.Ascending ? "ASC" : "DESC");

            var limitSql = String.Format("{0},{1}", (pagingSpec.Page - 1) * pagingSpec.PageSize, pagingSpec.PageSize);

            var currentTime = DateTime.UtcNow;
            _dataMapper.AddParameter("currentTime", currentTime);

            var sql = String.Format(@"SELECT Episodes.*, Series.Title as SeriesTitle
                                      FROM Episodes
                                      INNER JOIN Series
                                      ON Episodes.SeriesId = Series.Id
                                      WHERE EpisodeFileId = 0
                                      AND SeasonNumber > 0
                                      AND AirDate <= @currentTime
                                      ORDER BY {0}
                                      LIMIT {1}",
                                      orderSql, limitSql
                                   );

            pagingSpec.Records = _dataMapper.Query<Episode>(sql);
            pagingSpec.TotalRecords = Query.Count(e => e.EpisodeFileId == 0 && e.SeasonNumber > 0 && e.AirDate <= currentTime);

            return pagingSpec;
        }

        public Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber);
        }

        public List<Episode> EpisodesWithFiles()
        {
            return Query.Where(s => s.EpisodeFile != null).ToList();
        }

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate)
        {
            return Query//.Join<Episode, Series>(JoinType.None, e => e.Series, (e, s) => e.SeriesId == s.Id);
                          .Where<Episode>(e => e.AirDate >= startDate && e.AirDate <= endDate).ToList();
        }

        public void SetIgnoreFlat(Episode episode, bool ignoreFlag)
        {
            episode.Ignored = ignoreFlag;
            SetFields(episode, p => p.Ignored);
        }

        public void SetPostDownloadStatus(int episodeId, PostDownloadStatusType status)
        {
            SetFields(new Episode { Id = episodeId, PostDownloadStatus = status }, episode => episode.PostDownloadStatus);
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Episode { Id = episodeId, EpisodeFileId = fileId }, episode => episode.EpisodeFileId);
        }
    }
}
