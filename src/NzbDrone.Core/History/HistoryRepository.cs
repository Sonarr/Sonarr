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
        void DeleteForSeries(List<int> seriesIds);
        List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType);
        PagingSpec<EpisodeHistory> GetPaged(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities);
    }

    public class HistoryRepository : BasicRepository<EpisodeHistory>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public EpisodeHistory MostRecentForEpisode(int episodeId)
        {
            return Query(h => h.EpisodeId == episodeId).MaxBy(h => h.Date);
        }

        public List<EpisodeHistory> FindByEpisodeId(int episodeId)
        {
            return Query(h => h.EpisodeId == episodeId)
                        .OrderByDescending(h => h.Date)
                        .ToList();
        }

        public EpisodeHistory MostRecentForDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId).MaxBy(h => h.Date);
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

        public void DeleteForSeries(List<int> seriesIds)
        {
            Delete(c => seriesIds.Contains(c.SeriesId));
        }

        public List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                .Where<EpisodeHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<EpisodeHistory, Series, Episode>(builder, (history, series, episode) =>
            {
                history.Series = series;
                history.Episode = episode;
                return history;
            }).OrderBy(h => h.Date).ToList();
        }

        public PagingSpec<EpisodeHistory> GetPaged(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(pagingSpec, languages, qualities), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder(pagingSpec, languages, qualities).Select(typeof(EpisodeHistory)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        private SqlBuilder PagedBuilder(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id);

            AddFilters(builder, pagingSpec);

            if (languages is { Length: > 0 })
            {
                builder.Where($"({BuildLanguageWhereClause(languages)})");
            }

            if (qualities is { Length: > 0 })
            {
                builder.Where($"({BuildQualityWhereClause(qualities)})");
            }

            return builder;
        }

        protected override IEnumerable<EpisodeHistory> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<EpisodeHistory, Series, Episode>(builder, (history, series, episode) =>
            {
                history.Series = series;
                history.Episode = episode;
                return history;
            });

        private string BuildLanguageWhereClause(int[] languages)
        {
            var clauses = new List<string>();

            foreach (var language in languages)
            {
                // There are 4 different types of values we should see:
                // - Not the last value in the array
                // - When it's the last value in the array and on different OSes
                // - When it was converted from a single language

                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\".\"Languages\" LIKE '[% {language},%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\".\"Languages\" LIKE '[% {language}' || CHAR(13) || '%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\".\"Languages\" LIKE '[% {language}' || CHAR(10) || '%]'");
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\".\"Languages\" LIKE '[{language}]'");
            }

            return $"({string.Join(" OR ", clauses)})";
        }

        private string BuildQualityWhereClause(int[] qualities)
        {
            var clauses = new List<string>();

            foreach (var quality in qualities)
            {
                clauses.Add($"\"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\".\"Quality\" LIKE '%_quality_: {quality},%'");
            }

            return $"({string.Join(" OR ", clauses)})";
        }
    }
}
