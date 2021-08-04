using System;
using System.Collections.Generic;
using System.Linq;
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
            return Query(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();
        }

        public List<EpisodeHistory> FindByEpisodeId(int episodeId)
        {
            return Query(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .ToList();
        }

        public EpisodeHistory MostRecentForDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId)
             .OrderByDescending(h => h.Date)
             .FirstOrDefault();
        }

        public List<EpisodeHistory> FindByDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId);
        }

        public List<EpisodeHistory> GetBySeries(int seriesId, EpisodeHistoryEventType? eventType)
        {
            var builder = Builder().Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                                   .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                                   .Where<EpisodeHistory>(h => h.SeriesId == seriesId);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            return Query(builder).OrderByDescending(h => h.Date).ToList();
        }

        public List<EpisodeHistory> GetBySeason(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Where<EpisodeHistory>(h => h.SeriesId == seriesId && h.Episode.SeasonNumber == seasonNumber);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<EpisodeHistory, Episode>(
                builder,
                (history, episode) =>
                {
                    history.Episode = episode;
                    return history;
                }).OrderByDescending(h => h.Date).ToList();
        }

        public List<EpisodeHistory> FindDownloadHistory(int idSeriesId, QualityModel quality)
        {
            return Query(h =>
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

        protected override SqlBuilder PagedBuilder() => new SqlBuilder()
            .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
            .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id);

        protected override IEnumerable<EpisodeHistory> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<EpisodeHistory, Series, Episode>(builder, (history, series, episode) =>
            {
                history.Series = series;
                history.Episode = episode;
                return history;
            });

        public List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType)
        {
            var builder = Builder().Where<EpisodeHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            return Query(builder).OrderBy(h => h.Date).ToList();
        }
    }
}
