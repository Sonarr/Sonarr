using System;
using System.Collections.Generic;
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
        Episode Find(int seriesId, int season, int episodeNumber);
        Episode Get(int seriesId, DateTime date);
        Episode Find(int seriesId, DateTime date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials);
        Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        Episode FindEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> EpisodesWithFiles();
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate);
        void SetIgnoreFlat(Episode episode, bool ignoreFlag);
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

        public Episode Find(int seriesId, int season, int episodeNumber)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SeasonNumber == season && s.EpisodeNumber == episodeNumber);
        }

        public Episode Get(int seriesId, DateTime date)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.AirDate.HasValue && s.AirDate.Value.Date == date.Date);
        }

        public Episode Find(int seriesId, DateTime date)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.AirDate.HasValue && s.AirDate.Value.Date == date.Date);
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

        public PagingSpec<Episode> EpisodesWithoutFiles(PagingSpec<Episode> pagingSpec, bool includeSpecials)
        {
            var currentTime = DateTime.UtcNow;
            var startingSeasonNumber = 1;

            if (includeSpecials)
            {
                startingSeasonNumber = 0;
            }

            var pagingQuery = Query.Where(e => e.EpisodeFileId == 0)
                                   .AndWhere(e => e.SeasonNumber >= startingSeasonNumber)
                                   .AndWhere(e => e.AirDate <= currentTime)
                                   .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                                   .Skip(pagingSpec.PagingOffset())
                                   .Take(pagingSpec.PageSize);

            pagingSpec.Records = pagingQuery.ToList();

            //TODO: Use the same query for count and records
            pagingSpec.TotalRecords = Query.Count(e => e.EpisodeFileId == 0 && e.SeasonNumber >= startingSeasonNumber && e.AirDate <= currentTime);

            return pagingSpec;
        }

        public Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber);
        }

        public Episode FindEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber);
        }

        public List<Episode> EpisodesWithFiles()
        {
            return Query.Where(s => s.EpisodeFileId != 0).ToList();
        }

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate)
        {
            return Query.Where<Episode>(e => e.AirDate >= startDate)
                        .AndWhere(e => e.AirDate <= endDate).ToList();
        }

        public void SetIgnoreFlat(Episode episode, bool ignoreFlag)
        {
            episode.Ignored = ignoreFlag;
            SetFields(episode, p => p.Ignored);
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Episode { Id = episodeId, EpisodeFileId = fileId }, episode => episode.EpisodeFileId);
        }
    }
}
