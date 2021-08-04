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

        protected override SqlBuilder PagedBuilder() => new SqlBuilder().Join<Blocklist, Series>((b, m) => b.SeriesId == m.Id);
        protected override IEnumerable<Blocklist> PagedQuery(SqlBuilder sql) => _database.QueryJoined<Blocklist, Series>(sql, (bl, movie) =>
                    {
                        bl.Series = movie;
                        return bl;
                    });
    }
}
