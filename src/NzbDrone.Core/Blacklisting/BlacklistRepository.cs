using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Blacklisting
{
    public interface IBlacklistRepository : IBasicRepository<Blacklist>
    {
        bool Blacklisted(string sourceTitle);
    }

    public class BlacklistRepository : BasicRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(IDatabase database, IEventAggregator eventAggregator) :
            base(database, eventAggregator)
        {
        }

        public bool Blacklisted(string sourceTitle)
        {
            return Query.Any(e => e.SourceTitle.Contains(sourceTitle));
        }
    }
}
