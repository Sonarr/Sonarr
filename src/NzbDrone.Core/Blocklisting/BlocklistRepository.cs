using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Blocklisting
{
    public interface IBlocklistRepository : IBasicRepository<Blocklist>
    {
        Task<List<Blocklist>> BlocklistedByTitleAsync(int seriesId, string sourceTitle, CancellationToken cancellationToken = default);
        Task<List<Blocklist>> BlocklistedByTorrentInfoHashAsync(int seriesId, string torrentInfoHash, CancellationToken cancellationToken = default);
        Task<List<Blocklist>> BlocklistedBySeriesAsync(int seriesId, CancellationToken cancellationToken = default);
        Task DeleteForSeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default);
    }

    public class BlocklistRepository : BasicRepository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public async Task<List<Blocklist>> BlocklistedByTitleAsync(int seriesId, string sourceTitle, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(e => e.SeriesId == seriesId && e.SourceTitle.Contains(sourceTitle), cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Blocklist>> BlocklistedByTorrentInfoHashAsync(int seriesId, string torrentInfoHash, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(e => e.SeriesId == seriesId && e.TorrentInfoHash.Contains(torrentInfoHash), cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Blocklist>> BlocklistedBySeriesAsync(int seriesId, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(b => b.SeriesId == seriesId, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteForSeriesIdsAsync(List<int> seriesIds, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(x => seriesIds.Contains(x.SeriesId), cancellationToken).ConfigureAwait(false);
        }

        public override async Task<PagingSpec<Blocklist>> GetPagedAsync(PagingSpec<Blocklist> pagingSpec, CancellationToken cancellationToken = default)
        {
            pagingSpec.Records = await GetPagedRecordsAsync(PagedBuilder(), pagingSpec, PagedQueryAsync, cancellationToken).ConfigureAwait(false);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Blocklist))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = await GetPagedRecordCountAsync(PagedBuilder().Select(typeof(Blocklist)), pagingSpec, countTemplate, cancellationToken).ConfigureAwait(false);

            return pagingSpec;
        }

        protected override SqlBuilder PagedBuilder()
        {
            var builder = Builder()
                .Join<Blocklist, Series>((b, m) => b.SeriesId == m.Id);

            return builder;
        }

        protected override async Task<IEnumerable<Blocklist>> PagedQueryAsync(SqlBuilder builder, CancellationToken cancellationToken)
        {
            return await _database.QueryJoinedAsync<Blocklist, Series>(
                builder,
                (blocklist, series) =>
                {
                    blocklist.Series = series;
                    return blocklist;
                },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
