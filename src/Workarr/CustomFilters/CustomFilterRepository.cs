using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.CustomFilters
{
    public interface ICustomFilterRepository : IBasicRepository<CustomFilter>
    {
    }

    public class CustomFilterRepository : BasicRepository<CustomFilter>, ICustomFilterRepository
    {
        public CustomFilterRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
