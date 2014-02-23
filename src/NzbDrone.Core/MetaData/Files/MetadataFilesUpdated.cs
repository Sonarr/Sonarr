using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFilesUpdated : IEvent
    {
        public List<MetadataFile> MetadataFiles { get; set; }

        public MetadataFilesUpdated(List<MetadataFile> metadataFiles)
        {
            MetadataFiles = metadataFiles;
        }
    }
}
