using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Blocklisting
{
    public interface IBlocklistRepository : IBasicRepository<Blocklist>
    {
        List<Blocklist> BlocklistedByTitle(int seriesId, string sourceTitle);
        List<Blocklist> BlocklistedByTorrentInfoHash(int seriesId, string torrentInfoHash);
        List<Blocklist> BlocklistedBySeries(int seriesId);
        void DeleteForSeriesIds(List<int> seriesIds);
    }

    public class BlocklistRepository : BasicRepository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blocklist> BlocklistedByTitle(int seriesId, string sourceTitle)
        {
            return Query(e => e.SeriesId == seriesId && e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blocklist> BlocklistedByTorrentInfoHash(int seriesId, string torrentInfoHash)
        {
            return Query(e => e.SeriesId == seriesId && e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blocklist> BlocklistedBySeries(int seriesId)
        {
            return Query(b => b.SeriesId == seriesId);
        }

        public void DeleteForSeriesIds(List<int> seriesIds)
        {
            Delete(x => seriesIds.Contains(x.SeriesId));
        }

        public override PagingSpec<Blocklist> GetPaged(PagingSpec<Blocklist> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(PagedBuilder(), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM \"{TableMapping.Mapper.TableNameMapping(typeof(Blocklist))}\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/) AS \"Inner\"";
            pagingSpec.TotalRecords = GetPagedRecordCount(PagedBuilder().Select(typeof(Blocklist)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        protected override SqlBuilder PagedBuilder()
        {
            var builder = Builder()
                .Join<Blocklist, Series>((b, m) => b.SeriesId == m.Id);

            return builder;
        }

        protected override IEnumerable<Blocklist> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<Blocklist, Series>(builder, (blocklist, series) =>
            {
                blocklist.Series = series;
                return blocklist;
            });
    }
}
