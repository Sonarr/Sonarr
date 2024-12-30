using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;
    }
}
