using System;
using System.Collections.Generic;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Movies;
using NzbDrone.Api.REST;
using NzbDrone.Api.Series;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Queue
{
    public class QueueResource : RestResource
    {
        public SeriesResource Series { get; set; }
        public EpisodeResource Episode { get; set; }
        public MoviesResource Movie { get; set; }
        public QualityModel Quality { get; set; }
        public Decimal Size { get; set; }
        public String Title { get; set; }
        public Decimal Sizeleft { get; set; }
        public TimeSpan? Timeleft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public String Status { get; set; }
        public String TrackedDownloadStatus { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public String DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }
}
