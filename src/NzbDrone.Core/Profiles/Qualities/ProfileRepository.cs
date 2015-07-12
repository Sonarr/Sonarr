using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IProfileRepository : IBasicRepository<Profile>
    {
        bool Exists(int id);
    }

    public class ProfileRepository : BasicRepository<Profile>, IProfileRepository
    {
        public ProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool Exists(int id)
        {
            return DataMapper.Query<Profile>().Where(p => p.Id == id).GetRowCount() == 1;
        }
    }
}
