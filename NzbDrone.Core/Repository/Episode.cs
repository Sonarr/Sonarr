using System;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey]
        public string EpisodeId { get; set; }
        public long SeriesId { get; set; }
        public string Title { get; set; }
        public long Season { get; set; }
        public int EpisodeNumber { get; set; }
        public DateTime AirDate { get; set; }
        public Quality Quality { get; set; }
        public bool Proper { get; set; }
    }
}