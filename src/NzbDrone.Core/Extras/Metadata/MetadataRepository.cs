using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadataRepository : IProviderRepository<MetadataDefinition>
    {
    }

    public class MetadataRepository : ProviderRepository<MetadataDefinition>, IMetadataRepository
    {
        public MetadataRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
