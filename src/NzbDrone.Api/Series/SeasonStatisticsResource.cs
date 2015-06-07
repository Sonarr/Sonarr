using System;

namespace NzbDrone.Api.Series
{
    public class SeasonStatisticsResource
    {
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
        public int TotalEpisodeCount { get; set; }
        public long SizeOnDisk { get; set; }

        public decimal PercentOfEpisodes
        {
            get
            {
                if (EpisodeCount == 0) return 0;

                return (decimal)EpisodeFileCount / (decimal)EpisodeCount * 100;
            }
        }

    }
}
