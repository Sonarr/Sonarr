using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Events
{
    public class SeriesMetadataUpdated : IEvent
    {
        public Series Series { get; set; }
        public String Consumer { get; set; }
        public MetadataType MetadataType { get; set; }
        public String Path { get; set; }

        public SeriesMetadataUpdated(Series series, string consumer, MetadataType metadataType, string path)
        {
            Series = series;
            Consumer = consumer;
            MetadataType = metadataType;
            Path = path;
        }
    }
}
