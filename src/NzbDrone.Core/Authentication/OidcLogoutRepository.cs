using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Authentication
{
    public interface IOidcLogoutRepository : IBasicRepository<OidcLogout>
    {
    }

    public class OidcLogoutRepository : BasicRepository<OidcLogout>, IOidcLogoutRepository
    {
        public OidcLogoutRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
