using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.Profiles.Delay
{
    public interface IDelayProfileRepository : IBasicRepository<DelayProfile>
    {
    }

    public class DelayProfileRepository : BasicRepository<DelayProfile>, IDelayProfileRepository
    {
        public DelayProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
