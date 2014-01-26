using System;

namespace NzbDrone.Core.Metadata
{
    public enum MetadataType
    {
        Unknown = 0,
        SeriesMetadata = 1,
        EpisodeMetadata = 2,
        SeriesImage = 3,
        SeasonImage = 4,
        EpisodeImage = 5
    }
}
