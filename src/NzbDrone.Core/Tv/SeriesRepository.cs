using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        bool SeriesPathExists(string path);
        Series FindByTitle(string cleanTitle);
        Series FindByTitle(string cleanTitle, int year);
        List<Series> FindByTitleInexact(string cleanTitle);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        Series FindByImdbId(string imdbId);
        Series FindByPath(string path);
        List<int> AllSeriesTvdbIds();
        Dictionary<int, string> AllSeriesPaths();
        Dictionary<int, List<int>> AllSeriesTags();

        // Async
        Task<bool> SeriesPathExistsAsync(string path, CancellationToken cancellationToken = default);
        Task<Series> FindByTitleAsync(string cleanTitle, CancellationToken cancellationToken = default);
        Task<Series> FindByTitleAsync(string cleanTitle, int year, CancellationToken cancellationToken = default);
        Task<List<Series>> FindByTitleInexactAsync(string cleanTitle, CancellationToken cancellationToken = default);
        Task<Series> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default);
        Task<Series> FindByTvRageIdAsync(int tvRageId, CancellationToken cancellationToken = default);
        Task<Series> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
        Task<Series> FindByPathAsync(string path, CancellationToken cancellationToken = default);
        Task<List<int>> AllSeriesTvdbIdsAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<int, string>> AllSeriesPathsAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<int, List<int>>> AllSeriesTagsAsync(CancellationToken cancellationToken = default);
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool SeriesPathExists(string path)
        {
            return Query(c => c.Path == path).Any();
        }

        public Series FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle && s.Year == year).ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public List<Series> FindByTitleInexact(string cleanTitle)
        {
            var builder = Builder().Where($"instr(@cleanTitle, \"Series\".\"CleanTitle\")", new { cleanTitle = cleanTitle });

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                builder = Builder().Where($"(strpos(@cleanTitle, \"Series\".\"CleanTitle\") > 0)", new { cleanTitle = cleanTitle });
            }

            return Query(builder).ToList();
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query(s => s.TvdbId == tvdbId).SingleOrDefault();
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query(s => s.TvRageId == tvRageId).SingleOrDefault();
        }

        public Series FindByImdbId(string imdbId)
        {
            return Query(s => s.ImdbId == imdbId).SingleOrDefault();
        }

        public Series FindByPath(string path)
        {
            return Query(s => s.Path == path)
                        .FirstOrDefault();
        }

        public List<int> AllSeriesTvdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TvdbId\" FROM \"Series\"").ToList();
            }
        }

        public Dictionary<int, string> AllSeriesPaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Series\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public Dictionary<int, List<int>> AllSeriesTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Tags\" AS Value FROM \"Series\" WHERE \"Tags\" IS NOT NULL";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        private Series ReturnSingleSeriesOrThrow(List<Series> series)
        {
            if (series.Count == 0)
            {
                return null;
            }

            if (series.Count == 1)
            {
                return series.First();
            }

            throw new MultipleSeriesFoundException(series, "Expected one series, but found {0}. Matching series: {1}", series.Count, string.Join(", ", series));
        }

        // Async

        public async Task<bool> SeriesPathExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.Path == path, cancellationToken).ConfigureAwait(false);
            return results.Any();
        }

        public async Task<Series> FindByTitleAsync(string cleanTitle, CancellationToken cancellationToken = default)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = await QueryAsync(s => s.CleanTitle == cleanTitle, cancellationToken).ConfigureAwait(false);

            return ReturnSingleSeriesOrThrow(series);
        }

        public async Task<Series> FindByTitleAsync(string cleanTitle, int year, CancellationToken cancellationToken = default)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = await QueryAsync(s => s.CleanTitle == cleanTitle && s.Year == year, cancellationToken).ConfigureAwait(false);

            return ReturnSingleSeriesOrThrow(series);
        }

        public async Task<List<Series>> FindByTitleInexactAsync(string cleanTitle, CancellationToken cancellationToken = default)
        {
            var builder = Builder().Where($"instr(@cleanTitle, \"Series\".\"CleanTitle\")", new { cleanTitle = cleanTitle });

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                builder = Builder().Where($"(strpos(@cleanTitle, \"Series\".\"CleanTitle\") > 0)", new { cleanTitle = cleanTitle });
            }

            return await QueryAsync(builder, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Series> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(s => s.TvdbId == tvdbId, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task<Series> FindByTvRageIdAsync(int tvRageId, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(s => s.TvRageId == tvRageId, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task<Series> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(s => s.ImdbId == imdbId, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task<Series> FindByPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(s => s.Path == path, cancellationToken).ConfigureAwait(false);
            return results.FirstOrDefault();
        }

        public async Task<List<int>> AllSeriesTvdbIdsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition("SELECT \"TvdbId\" FROM \"Series\"", cancellationToken: cancellationToken);
            var results = await conn.QueryAsync<int>(cmd).ConfigureAwait(false);
            return results.ToList();
        }

        public async Task<Dictionary<int, string>> AllSeriesPathsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition("SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Series\"", cancellationToken: cancellationToken);
            var results = await conn.QueryAsync<KeyValuePair<int, string>>(cmd).ConfigureAwait(false);
            return results.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<Dictionary<int, List<int>>> AllSeriesTagsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            var cmd = new CommandDefinition("SELECT \"Id\" AS Key, \"Tags\" AS Value FROM \"Series\" WHERE \"Tags\" IS NOT NULL", cancellationToken: cancellationToken);
            var results = await conn.QueryAsync<KeyValuePair<int, List<int>>>(cmd).ConfigureAwait(false);
            return results.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
