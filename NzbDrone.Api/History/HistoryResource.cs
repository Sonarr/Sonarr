using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.History
{
    public class HistoryResource : RestResource
    {
        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public string Indexer { get; set; }
        public string NzbInfoUrl { get; set; }
        public string ReleaseGroup { get; set; }

        public Episode Episode { get; set; }
        public Core.Tv.Series Series { get; set; } 
    }
}
