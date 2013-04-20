using System;

namespace NzbDrone.Core.Tv
{
    public class SeriesStatistics
    {
        public int SeriesId { get; set; }
        public int NumberOfSeasons { get; set; }
        public DateTime? NextAiring { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
    }
}
