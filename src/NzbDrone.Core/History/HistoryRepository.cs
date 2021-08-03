using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<EpisodeHistory>
    {
        EpisodeHistory MostRecentForEpisode(int episodeId);
        List<EpisodeHistory> FindByEpisodeId(int episodeId);
        EpisodeHistory MostRecentForDownloadId(string downloadId);
        List<EpisodeHistory> FindByDownloadId(string downloadId);
        List<EpisodeHistory> GetBySeries(int seriesId, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> GetBySeason(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> FindDownloadHistory(int idSeriesId, QualityModel quality);
        void DeleteForSeries(int seriesId);
        List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<EpisodeHistory>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public EpisodeHistory MostRecentForEpisode(int episodeId)
        {
            return Query.Where(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();
        }

        public List<EpisodeHistory> FindByEpisodeId(int episodeId)
        {
            return Query.Where(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .ToList();
        }

        public EpisodeHistory MostRecentForDownloadId(string downloadId)
        {
            return Query.Where(h => h.DownloadId == downloadId)
             .OrderByDescending(h => h.Date)
             .FirstOrDefault();
        }

        public List<EpisodeHistory> FindByDownloadId(string downloadId)
        {
            return Query.Where(h => h.DownloadId == downloadId);
        }

        public List<EpisodeHistory> GetBySeries(int seriesId, EpisodeHistoryEventType? eventType)
        {
            var query = Query.Join<EpisodeHistory, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                             .Join<EpisodeHistory, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id)
                             .Where(h => h.SeriesId == seriesId);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            return query.OrderByDescending(h => h.Date).ToList();
        }

        public List<EpisodeHistory> GetBySeason(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType)
        {
            SortBuilder<EpisodeHistory> query = Query
                        .Join<EpisodeHistory, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id)
                        .Join<EpisodeHistory, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                        .Where(h => h.SeriesId == seriesId)
                        .AndWhere(h => h.Episode.SeasonNumber == seasonNumber);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public List<EpisodeHistory> FindDownloadHistory(int idSeriesId, QualityModel quality)
        {
            return Query.Where(h =>
                 h.SeriesId == idSeriesId &&
                 h.Quality == quality &&
                 (h.EventType == EpisodeHistoryEventType.Grabbed ||
                 h.EventType == EpisodeHistoryEventType.DownloadFailed ||
                 h.EventType == EpisodeHistoryEventType.DownloadFolderImported))
                 .ToList();
        }

        public void DeleteForSeries(int seriesId)
        {
            Delete(c => c.SeriesId == seriesId);
        }

        protected override SortBuilder<EpisodeHistory> GetPagedQuery(QueryBuilder<EpisodeHistory> query, PagingSpec<EpisodeHistory> pagingSpec)
        {
            var baseQuery = query.Join<EpisodeHistory, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                                 .Join<EpisodeHistory, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }

        public List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType)
        {
            var query = Query.Where(h => h.Date >= date);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderBy(h => h.Date);

            return query;
        }
    }
}
