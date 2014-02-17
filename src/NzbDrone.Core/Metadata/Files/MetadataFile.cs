using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFile : ModelBase
    {
        public Int32 SeriesId { get; set; }
        public String Consumer { get; set; }
        public MetadataType Type { get; set; }
        public String RelativePath { get; set; }
        public DateTime LastUpdated { get; set; }
        public Int32? EpisodeFileId { get; set; }
        public Int32? SeasonNumber { get; set; }
    }
}
