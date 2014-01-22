using System;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Events
{
    public class SeasonMetadataUpdated : IEvent
    {
        public Series Series { get; set; }
        public Int32 SeasonNumber { get; set; }
        public String Consumer { get; set; }
        public MetadataType MetadataType { get; set; }
        public String Path { get; set; }

        public SeasonMetadataUpdated(Series series, int seasonNumber, string consumer, MetadataType metadataType, string path)
        {
            Series = series;
            SeasonNumber = seasonNumber;
            Consumer = consumer;
            MetadataType = metadataType;
            Path = path;
        }
    }
}
