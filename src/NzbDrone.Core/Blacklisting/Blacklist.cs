using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Blacklisting
{
    public class Blacklist : ModelBase
    {
        public Int32 SeriesId { get; set; }
        public Series Series { get; set; }
        public List<Int32> EpisodeIds { get; set; }
        public String SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PublishedDate { get; set; }
    }
}
