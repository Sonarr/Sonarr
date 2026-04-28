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
        Task<bool> SeriesPathExistsAsync(string path, CancellationToken cancellationToken = default);
        Series FindByTitle(string cleanTitle);
        Task<Series> FindByTitleAsync(string cleanTitle, CancellationToken cancellationToken = default);
        Series FindByTitle(string cleanTitle, int year);
        Task<Series> FindByTitleAsync(string cleanTitle, int year, CancellationToken cancellationToken = default);
        List<Series> FindByTitleInexact(string cleanTitle);
        Task<List<Series>> FindByTitleInexactAsync(string cleanTitle, CancellationToken cancellationToken = default);
        Series FindByTvdbId(int tvdbId);
        Task<Series> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default);
        Series FindByTvRageId(int tvRageId);
        Task<Series> FindByTvRageIdAsync(int tvRageId, CancellationToken cancellationToken = default);
        Series FindByImdbId(string imdbId);
        Task<Series> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
        Series FindByPath(string path);
        Task<Series> FindByPathAsync(string path, CancellationToken cancellationToken = default);
        List<int> AllSeriesTvdbIds();
        Task<List<int>> AllSeriesTvdbIdsAsync(CancellationToken cancellationToken = default);
        Dictionary<int, string> AllSeriesPaths();
        Task<Dictionary<int, string>> AllSeriesPathsAsync(CancellationToken cancellationToken = default);
        Dictionary<int, List<int>> AllSeriesTags();
        Task<Dictionary<int, List<int>>> AllSeriesTagsAsync(CancellationToken cancellationToken = default);
        Dictionary<int, int> AllSeriesQualityProfiles();
        Task<Dictionary<int, int>> AllSeriesQualityProfilesAsync(CancellationToken cancellationToken = default);
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

        public async Task<bool> SeriesPathExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(c => c.Path == path, cancellationToken).AnyAsync(cancellationToken);
        }

        public Series FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle)
                                        .ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public async Task<Series> FindByTitleAsync(string cleanTitle, CancellationToken cancellationToken = default)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = await QueryAsync(s => s.CleanTitle == cleanTitle, cancellationToken).ToListAsync(cancellationToken);

            return ReturnSingleSeriesOrThrow(series);
        }

        public Series FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = Query(s => s.CleanTitle == cleanTitle && s.Year == year).ToList();

            return ReturnSingleSeriesOrThrow(series);
        }

        public async Task<Series> FindByTitleAsync(string cleanTitle, int year, CancellationToken cancellationToken = default)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var series = await QueryAsync(s => s.CleanTitle == cleanTitle && s.Year == year, cancellationToken).ToListAsync(cancellationToken);

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

        public async Task<List<Series>> FindByTitleInexactAsync(string cleanTitle, CancellationToken cancellationToken = default)
        {
            var builder = Builder().Where("instr(@cleanTitle, \"Series\".\"CleanTitle\")", new { cleanTitle = cleanTitle });

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                builder = Builder().Where("(strpos(@cleanTitle, \"Series\".\"CleanTitle\") > 0)", new { cleanTitle = cleanTitle });
            }

            return await QueryAsync(builder, cancellationToken).ToListAsync(cancellationToken);
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return Query(s => s.TvdbId == tvdbId).SingleOrDefault();
        }

        public async Task<Series> FindByTvdbIdAsync(int tvdbId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(s => s.TvdbId == tvdbId, cancellationToken).SingleOrDefaultAsync(cancellationToken);
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return Query(s => s.TvRageId == tvRageId).SingleOrDefault();
        }

        public async Task<Series> FindByTvRageIdAsync(int tvRageId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(s => s.TvRageId == tvRageId, cancellationToken).SingleOrDefaultAsync(cancellationToken);
        }

        public Series FindByImdbId(string imdbId)
        {
            return Query(s => s.ImdbId == imdbId).SingleOrDefault();
        }

        public async Task<Series> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(s => s.ImdbId == imdbId, cancellationToken).SingleOrDefaultAsync(cancellationToken);
        }

        public Series FindByPath(string path)
        {
            return Query(s => s.Path == path)
                        .FirstOrDefault();
        }

        public async Task<Series> FindByPathAsync(string path, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(s => s.Path == path, cancellationToken).SingleOrDefaultAsync(cancellationToken);
        }

        public List<int> AllSeriesTvdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TvdbId\" FROM \"Series\"").ToList();
            }
        }

        public async Task<List<int>> AllSeriesTvdbIdsAsync(CancellationToken cancellationToken = default)
        {
            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            return await conn.QueryUnbufferedAsync<int>("SELECT \"TvdbId\" FROM \"Series\"").ToListAsync(cancellationToken);
        }

        public Dictionary<int, string> AllSeriesPaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Series\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public async Task<Dictionary<int, string>> AllSeriesPathsAsync(CancellationToken cancellationToken = default)
        {
            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            return await conn.QueryUnbufferedAsync<KeyValuePair<int, string>>("SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Series\"")
                .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken: cancellationToken);
        }

        public Dictionary<int, List<int>> AllSeriesTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Tags\" AS Value FROM \"Series\" WHERE \"Tags\" IS NOT NULL";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public async Task<Dictionary<int, List<int>>> AllSeriesTagsAsync(CancellationToken cancellationToken = default)
        {
            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            return await conn.QueryUnbufferedAsync<KeyValuePair<int, List<int>>>("SELECT \"Id\" AS Key, \"Tags\" AS Value FROM \"Series\" WHERE \"Tags\" IS NOT NULL")
                .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken: cancellationToken);
        }

        public Dictionary<int, int> AllSeriesQualityProfiles()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"QualityProfileId\" AS Value FROM \"Series\"";
                return conn.Query<KeyValuePair<int, int>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public async Task<Dictionary<int, int>> AllSeriesQualityProfilesAsync(CancellationToken cancellationToken = default)
        {
            await using var conn = await _database.OpenConnectionAsync(cancellationToken);

            return await conn.QueryUnbufferedAsync<KeyValuePair<int, int>>("SELECT \"Id\" AS Key, \"QualityProfileId\" AS Value FROM \"Series\"")
                .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken: cancellationToken);
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
    }
}
