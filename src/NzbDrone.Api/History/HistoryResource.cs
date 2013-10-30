using System;
using System.Collections.Generic;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Core.History;
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

        public HistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public EpisodeResource Episode { get; set; }
        public SeriesResource Series { get; set; }
    }
}
