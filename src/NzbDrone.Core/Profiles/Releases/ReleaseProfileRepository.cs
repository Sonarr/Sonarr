using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IRestrictionRepository : IBasicRepository<ReleaseProfile>
    {
    }

    public class ReleaseProfileRepository : BasicRepository<ReleaseProfile>, IRestrictionRepository
    {
        public ReleaseProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
