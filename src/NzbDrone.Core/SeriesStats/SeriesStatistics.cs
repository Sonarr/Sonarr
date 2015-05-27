using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public class SeriesStatistics : ResultSet
    {
        public int SeriesId { get; set; }
        public string NextAiringString { get; set; }
        public string PreviousAiringString { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
        public int TotalEpisodeCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<SeasonStatistics> SeasonStatistics { get; set; }

        public DateTime? NextAiring
        {
            get
            {
                DateTime nextAiring;

                if (!DateTime.TryParse(NextAiringString, out nextAiring)) return null;

                return nextAiring;
            }
        }

        public DateTime? PreviousAiring
        {
            get
            {
                DateTime previousAiring;

                if (!DateTime.TryParse(PreviousAiringString, out previousAiring)) return null;

                return previousAiring;
            }
        }
    }
}
