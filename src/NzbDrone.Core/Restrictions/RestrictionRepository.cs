using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Restrictions
{
    public interface IRestrictionRepository : IBasicRepository<Restriction>
    {
    }

    public class RestrictionRepository : BasicRepository<Restriction>, IRestrictionRepository
    {
        public RestrictionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
