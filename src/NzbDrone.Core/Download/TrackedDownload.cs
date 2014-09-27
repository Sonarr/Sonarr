using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download
{
    public class TrackedDownload
    {
        public String TrackingId { get; set; }
        public Int32 DownloadClient { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public TrackedDownloadState State { get; set; }
        public DateTime StartedTracking { get; set; }
        public DateTime LastRetry { get; set; }
        public Int32 RetryCount { get; set; }
        public Boolean HasError { get; set; }
        public String StatusMessage { get; set; }
        public List<String> StatusMessages { get; set; }
    }

    public enum TrackedDownloadState
    {
        Unknown,
        Downloading,
        Imported,
        DownloadFailed,
        Removed
    }
}
