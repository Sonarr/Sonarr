using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class SeriesStatistics : ResultSet
    {
        public int SeriesId { get; set; }
        public int NumberOfSeasons { get; set; }
        public string NextAiringString { get; set; }
        public DateTime? NextAiring
        {
            get { return Convert.ToDateTime(NextAiringString); }
        }
        public int EpisodeFileCount { get; set; }
        public int EpisodeCount { get; set; }
    }
}
