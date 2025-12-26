using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<EpisodeHistory>
    {
        Task<EpisodeHistory> MostRecentForEpisodeAsync(int episodeId, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> FindByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default);
        Task<EpisodeHistory> MostRecentForDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> FindByDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> GetBySeriesAsync(int seriesId, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> GetBySeasonAsync(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> GetByEpisodeAsync(int episodeId, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> FindDownloadHistoryAsync(int idSeriesId, QualityModel quality, CancellationToken cancellationToken = default);
        Task DeleteForSeriesAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
        Task<List<EpisodeHistory>> SinceAsync(DateTime date, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default);
        Task<PagingSpec<EpisodeHistory>> GetPagedAsync(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities, CancellationToken cancellationToken = default);
    }

    public class HistoryRepository : BasicRepository<EpisodeHistory>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        private SqlBuilder PagedBuilder(int[] languages, int[] qualities)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id);

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

        private string BuildLanguageWhereClause(int[] languages)
        {
            var clauses = new List<string>();

            foreach (var language in languages)
            {
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

        public async Task<EpisodeHistory> MostRecentForEpisodeAsync(int episodeId, CancellationToken cancellationToken = default)
        {
            var histories = await QueryAsync(h => h.EpisodeId == episodeId, cancellationToken).ConfigureAwait(false);
            return histories.MaxBy(h => h.Date);
        }

        public async Task<List<EpisodeHistory>> FindByEpisodeIdAsync(int episodeId, CancellationToken cancellationToken = default)
        {
            var episodeHistories = await QueryAsync(h => h.EpisodeId == episodeId, cancellationToken).ConfigureAwait(false);
            return episodeHistories.OrderByDescending(h => h.Date).ToList();
        }

        public async Task<EpisodeHistory> MostRecentForDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default)
        {
            var histories = await QueryAsync(h => h.DownloadId == downloadId, cancellationToken).ConfigureAwait(false);
            return histories.MaxBy(h => h.Date);
        }

        public async Task<List<EpisodeHistory>> FindByDownloadIdAsync(string downloadId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(h => h.DownloadId == downloadId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeHistory>> GetBySeriesAsync(int seriesId, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default)
        {
            var builder = Builder().Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                                   .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                                   .Where<EpisodeHistory>(h => h.SeriesId == seriesId);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            var episodeHistories = await QueryAsync(builder, cancellationToken).ConfigureAwait(false);
            return episodeHistories.OrderByDescending(h => h.Date).ToList();
        }

        public async Task<List<EpisodeHistory>> GetBySeasonAsync(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Where<EpisodeHistory>(h => h.SeriesId == seriesId && h.Episode.SeasonNumber == seasonNumber);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            var results = await _database.QueryJoinedAsync<EpisodeHistory, Episode>(
                builder,
                (history, episode) =>
                {
                    history.Episode = episode;
                    return history;
                },
                cancellationToken).ConfigureAwait(false);

            return results.OrderByDescending(h => h.Date).ToList();
        }

        public async Task<List<EpisodeHistory>> GetByEpisodeAsync(int episodeId, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                .Where<EpisodeHistory>(h => h.EpisodeId == episodeId);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            var episodeHistories = await QueryAsync(builder, cancellationToken).ConfigureAwait(false);
            return episodeHistories.OrderByDescending(h => h.Date).ToList();
        }

        public async Task<List<EpisodeHistory>> FindDownloadHistoryAsync(int idSeriesId, QualityModel quality, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(h =>
                 h.SeriesId == idSeriesId &&
                 h.Quality == quality &&
                 (h.EventType == EpisodeHistoryEventType.Grabbed ||
                 h.EventType == EpisodeHistoryEventType.DownloadFailed ||
                 h.EventType == EpisodeHistoryEventType.DownloadFolderImported),
                 cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteForSeriesAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(c => seriesIds.Contains(c.SeriesId), cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<EpisodeHistory>> SinceAsync(DateTime date, EpisodeHistoryEventType? eventType, CancellationToken cancellationToken = default)
        {
            var builder = Builder()
                .Join<EpisodeHistory, Series>((h, a) => h.SeriesId == a.Id)
                .Join<EpisodeHistory, Episode>((h, a) => h.EpisodeId == a.Id)
                .Where<EpisodeHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<EpisodeHistory>(h => h.EventType == eventType);
            }

            var results = await _database.QueryJoinedAsync<EpisodeHistory, Series, Episode>(
                builder,
                (history, series, episode) =>
                {
                    history.Series = series;
                    history.Episode = episode;
                    return history;
                },
                cancellationToken).ConfigureAwait(false);

            return results.OrderBy(h => h.Date).ToList();
        }

        public async Task<PagingSpec<EpisodeHistory>> GetPagedAsync(PagingSpec<EpisodeHistory> pagingSpec, int[] languages, int[] qualities, CancellationToken cancellationToken = default)
        {
            pagingSpec.Records = await GetPagedRecordsAsync(PagedBuilder(languages, qualities), pagingSpec, PagedQueryAsync, cancellationToken).ConfigureAwait(false);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(EpisodeHistory))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = await GetPagedRecordCountAsync(PagedBuilder(languages, qualities).Select(typeof(EpisodeHistory)), pagingSpec, countTemplate, cancellationToken).ConfigureAwait(false);

            return pagingSpec;
        }

        protected override async Task<IEnumerable<EpisodeHistory>> PagedQueryAsync(SqlBuilder builder, CancellationToken cancellationToken)
        {
            return await _database.QueryJoinedAsync<EpisodeHistory, Series, Episode>(
                builder,
                (history, series, episode) =>
                {
                    history.Series = series;
                    history.Episode = episode;
                    return history;
                },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
