using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.AutoTagging
{
    public interface IAutoTaggingRepository : IBasicRepository<AutoTag>
    {
    }

    public class AutoTaggingRepository : BasicRepository<AutoTag>, IAutoTaggingRepository
    {
        public AutoTaggingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
