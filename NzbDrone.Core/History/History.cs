using System.Linq;
using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public class History : ModelBase
    {
        public int EpisodeId { get; set; }
        public string NzbTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public string Indexer { get; set; }
        public string NzbInfoUrl { get; set; }
        public string ReleaseGroup { get; set; }

    }
}