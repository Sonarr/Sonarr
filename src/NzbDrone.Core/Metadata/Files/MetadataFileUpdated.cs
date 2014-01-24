using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFileUpdated : IEvent
    {
        public MetadataFile Metadata { get; set; }

        public MetadataFileUpdated(MetadataFile metadata)
        {
            Metadata = metadata;
        }
    }
}
