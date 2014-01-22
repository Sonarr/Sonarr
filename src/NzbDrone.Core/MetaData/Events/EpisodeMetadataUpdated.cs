using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Events
{
    public class EpisodeMetadataUpdated : IEvent
    {
        public Series Series { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public String Consumer { get; set; }
        public MetadataType MetadataType { get; set; }
        public String Path { get; set; }

        public EpisodeMetadataUpdated(Series series, EpisodeFile episodeFile, string consumer, MetadataType metadataType, string path)
        {
            Series = series;
            EpisodeFile = episodeFile;
            Consumer = consumer;
            MetadataType = metadataType;
            Path = path;
        }
    }
}
