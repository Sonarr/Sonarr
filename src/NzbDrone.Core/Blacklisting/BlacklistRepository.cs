using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using Marr.Data.QGen;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        List<Blacklist> Blacklisted(int seriesId, string sourceTitle);
        List<Blacklist> BlacklistedBySeries(int seriesId);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IMainDatabase database, IEventAggregator eventAggregator) :
            base(database, eventAggregator)
        {
        }

        public List<Blacklist> Blacklisted(int seriesId, string sourceTitle)
        {
            return Query.Where(e => e.SeriesId == seriesId)
                        .AndWhere(e => e.SourceTitle.Contains(sourceTitle));
        }

        public List<Blacklist> BlacklistedBySeries(int seriesId)
        {
            return Query.Where(b => b.SeriesId == seriesId);
        }

        protected override SortBuilder<Blacklist> GetPagedQuery(QueryBuilder<Blacklist> query, PagingSpec<Blacklist> pagingSpec)
        {
            var baseQuery = query.Join<Blacklist, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }
    }
}
