using System;
using System.Collections.Generic;
using NzbDrone.Api.Movies;
using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public int MovieId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public SeriesResource Series { get; set; }
        public MoviesResource Movie { get; set; }
    }
}
