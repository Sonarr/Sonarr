using System;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey]
        public string EpisodeId { get; set; }
        public string SeriesId { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }
        public int EpisodeNumber2 { get; set; }
        public DateTime AirDate { get; set; }
        public string Release { get; set; }
        public int Quality { get; set; }
        public bool Proper { get; set; }
        public String FileName { get; set; }
        public  SyndicationItem Feed { get; set; }
    }
}
