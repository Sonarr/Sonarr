using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Metadata.Files
{
    public interface IMetadataFileRepository : IExtraFileRepository<MetadataFile>
    {
    }

    public class MetadataFileRepository : ExtraFileRepository<MetadataFile>, IMetadataFileRepository
    {
        public MetadataFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
