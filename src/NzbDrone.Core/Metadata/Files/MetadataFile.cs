using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFile : ModelBase
    {
        public int SeriesId { get; set; }
        public string Consumer { get; set; }
        public MetadataType Type { get; set; }
        public string RelativePath { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? EpisodeFileId { get; set; }
        public int? SeasonNumber { get; set; }
        public string Hash { get; set; }
    }
}
