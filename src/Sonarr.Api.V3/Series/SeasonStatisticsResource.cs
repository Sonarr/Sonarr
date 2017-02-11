using System;
using NzbDrone.Core.SeriesStats;

namespace Sonarr.Api.V3.Series
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

    public static class SeasonStatisticsResourceMapper
    {
        public static SeasonStatisticsResource ToResource(this SeasonStatistics model)
        {
            if (model == null) return null;

            return new SeasonStatisticsResource
            {
                NextAiring = model.NextAiring,
                PreviousAiring = model.PreviousAiring,
                EpisodeFileCount = model.EpisodeFileCount,
                EpisodeCount = model.EpisodeCount,
                TotalEpisodeCount = model.TotalEpisodeCount,
                SizeOnDisk = model.SizeOnDisk
            };
        }
    }
}
