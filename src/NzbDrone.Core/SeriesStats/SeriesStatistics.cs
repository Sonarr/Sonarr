using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.SeriesStats
{
    public class SeriesStatistics : ResultSet
    {
        public Int32 SeriesId { get; set; }
        public String NextAiringString { get; set; }
        public String PreviousAiringString { get; set; }
        public Int32 EpisodeFileCount { get; set; }
        public Int32 EpisodeCount { get; set; }
        public Int64 SizeOnDisk { get; set; }

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
