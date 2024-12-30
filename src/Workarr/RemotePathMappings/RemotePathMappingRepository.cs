using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.RemotePathMappings
{
    public interface IRemotePathMappingRepository : IBasicRepository<RemotePathMapping>
    {
    }

    public class RemotePathMappingRepository : BasicRepository<RemotePathMapping>, IRemotePathMappingRepository
    {
        public RemotePathMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;
    }
}
