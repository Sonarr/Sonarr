using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public class SeriesStatistics : ResultSet
    {
        public int SeriesId { get; set; }
        public string NextAiringString { get; set; }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }

        public DateTime? NextAiring
        {
            get
            {
                DateTime nextAiring;

                if (!DateTime.TryParse(NextAiringString, out nextAiring)) return null;

                return nextAiring;
            }
        }
    }
}
