using System.Collections.Generic;
using Marr.Data.QGen;
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
    }

    public class BlocklistRepository : BasicRepository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Blocklist> BlocklistedByTitle(int seriesId, string sourceTitle)
        {
            return Query.Where(e => e.SeriesId == seriesId)
                        .AndWhere(e => e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blocklist> BlocklistedByTorrentInfoHash(int seriesId, string torrentInfoHash)
        {
            return Query.Where(e => e.SeriesId == seriesId)
                        .AndWhere(e => e.TorrentInfoHash.Contains(torrentInfoHash));
        }

        public List<Blocklist> BlocklistedBySeries(int seriesId)
        {
            return Query.Where(b => b.SeriesId == seriesId);
        }

        protected override SortBuilder<Blocklist> GetPagedQuery(QueryBuilder<Blocklist> query, PagingSpec<Blocklist> pagingSpec)
        {
            var baseQuery = query.Join<Blocklist, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }
    }
}
