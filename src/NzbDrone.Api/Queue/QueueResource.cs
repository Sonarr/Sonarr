using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Download;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Series;
using NzbDrone.Api.Episodes;

namespace NzbDrone.Api.Queue
{
    public class QueueResource : RestResource
    {
        public SeriesResource Series { get; set; }
        public EpisodeResource Episode { get; set; }
        public QualityModel Quality { get; set; }
        public Decimal Size { get; set; }
        public String Title { get; set; }
        public Decimal Sizeleft { get; set; }
        public TimeSpan? Timeleft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public String Status { get; set; }
        public String TrackedDownloadStatus { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public String TrackingId { get; set; }
    }
}
