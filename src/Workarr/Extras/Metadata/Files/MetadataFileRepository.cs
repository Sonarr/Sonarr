using Workarr.Datastore;
using Workarr.Extras.Files;
using Workarr.Messaging.Events;

namespace Workarr.Extras.Metadata.Files
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
