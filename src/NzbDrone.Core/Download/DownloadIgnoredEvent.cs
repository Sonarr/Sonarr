using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Download
{
    public class DownloadIgnoredEvent : IEvent
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public TrackedDownload TrackedDownload { get; set; }
        public string Message { get; set; }
    }
}
