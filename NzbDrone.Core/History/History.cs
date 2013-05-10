using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public class History : ModelBase
    {
        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string NzbTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public string Indexer { get; set; }
        public string NzbInfoUrl { get; set; }
        public string ReleaseGroup { get; set; }

//        public LazyLoaded<Episode> Episode { get; set; }
//        public LazyLoaded<Series> Series { get; set; }

        public Episode Episode { get; set; }
        public Series Series { get; set; }
    }
}