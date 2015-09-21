using System.Collections.Generic;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> BlacklistedByTitle(Media media, string sourceTitle);
        List<Blacklist> BlacklistedByTorrentInfoHash(Media media, string torrentInfoHash);
        List<Blacklist> BlacklistedBySeries(int seriesId);
        List<Blacklist> BlacklistedByMovie(int movieId);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator) :
            base(database, eventAggregator)
        {
        }

        public List<Blacklist> BlacklistedByTitle(Media media, string sourceTitle)
        {
            if (media is Series)
            {
                return Query.Where(e => e.SeriesId > 0)
                            .AndWhere(e => e.SourceTitle.Contains(sourceTitle));
            }
            else if (media is Movie)
            {
                return Query.Where(e => e.MovieId > 0)
                            .AndWhere(e => e.SourceTitle.Contains(sourceTitle));
            }
            return null;

        }

        public List<Blacklist> BlacklistedByTorrentInfoHash(Media media, string torrentInfoHash)
        {
            if (media is Series)
            {
                return Query.Where(e => e.SeriesId == media.Id)
                            .AndWhere(e => e.TorrentInfoHash.Contains(torrentInfoHash));
            }
            else if (media is Movie)
            {
                return Query.Where(e => e.MovieId == media.Id)
                            .AndWhere(e => e.TorrentInfoHash.Contains(torrentInfoHash));
            }
            return null;
        }

        public List<Blacklist> BlacklistedBySeries(int seriesId)
        {
            return Query.Where(b => b.SeriesId == seriesId);
        }

        public List<Blacklist> BlacklistedByMovie(int movieId)
        {
            return Query.Where(b => b.MovieId == movieId);
        }

        protected override SortBuilder<Blacklist> GetPagedQuery(QueryBuilder<Blacklist> query, PagingSpec<Blacklist> pagingSpec)
        {
            var baseQuery = query.Join<Blacklist, Series>(JoinType.Left, h => h.Series, (h, s) => h.SeriesId == s.Id)
                                 .Join<Blacklist, Movie>(JoinType.Left, h => h.Movie, (h, s) => h.MovieId == s.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }
    }
}
