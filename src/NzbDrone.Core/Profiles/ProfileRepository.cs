using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles
{
    public interface IProfileRepository : IBasicRepository<Profile>
    {
        
    }

    public class ProfileRepository : BasicRepository<Profile>, IProfileRepository
    {
        public ProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
