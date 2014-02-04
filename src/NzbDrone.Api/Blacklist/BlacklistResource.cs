using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
    }
}
