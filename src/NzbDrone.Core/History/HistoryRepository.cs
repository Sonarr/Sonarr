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
    public interface IHistoryRepository : IBasicRepository<History>
    {
        History MostRecentForEpisode(int episodeId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> GetBySeries(int seriesId, HistoryEventType? eventType);
        List<History> GetBySeason(int seriesId, int seasonNumber, HistoryEventType? eventType);
        List<History> FindDownloadHistory(int idSeriesId, QualityModel quality);
        void DeleteForSeries(int seriesId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {

        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public History MostRecentForEpisode(int episodeId)
        {
            return Query.Where(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return Query.Where(h => h.DownloadId == downloadId)
             .OrderByDescending(h => h.Date)
             .FirstOrDefault();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return Query.Where(h => h.DownloadId == downloadId);
        }

        public List<History> GetBySeries(int seriesId, HistoryEventType? eventType)
        {
            var query = Query.Where(h => h.SeriesId == seriesId);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public List<History> GetBySeason(int seriesId, int seasonNumber, HistoryEventType? eventType)
        {
            var query = Query.Join<History, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id)
                             .Where(h => h.SeriesId == seriesId)
                             .AndWhere(h => h.Episode.SeasonNumber == seasonNumber);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public List<History> FindDownloadHistory(int idSeriesId, QualityModel quality)
        {
            return Query.Where(h =>
                 h.SeriesId == idSeriesId &&
                 h.Quality == quality &&
                 (h.EventType == HistoryEventType.Grabbed ||
                 h.EventType == HistoryEventType.DownloadFailed ||
                 h.EventType == HistoryEventType.DownloadFolderImported)
                 ).ToList();
        }

        public void DeleteForSeries(int seriesId)
        {
            Delete(c => c.SeriesId == seriesId);
        }

        protected override SortBuilder<History> GetPagedQuery(QueryBuilder<History> query, PagingSpec<History> pagingSpec)
        {
            var baseQuery = query.Join<History, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                                 .Join<History, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }

        public List<History> Since(DateTime date, HistoryEventType? eventType)
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
