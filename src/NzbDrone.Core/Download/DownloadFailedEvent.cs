using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Download
{
    public class DownloadFailedEvent : IEvent
    {
        public DownloadFailedEvent()
        {
            Data = new Dictionary<string, string>();
        }

        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public Language Language { get; set; }
    }
}