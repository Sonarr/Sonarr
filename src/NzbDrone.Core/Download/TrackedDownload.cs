using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class TrackedDownload
    {
        public String TrackingId { get; set; }
        public Int32 DownloadClient { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public TrackedDownloadState State { get; set; }
        public TrackedDownloadStatus Status { get; set; }
        public DateTime StartedTracking { get; set; }
        public DateTime LastRetry { get; set; }
        public Int32 RetryCount { get; set; }
        public String StatusMessage { get; set; }
        public RemoteEpisode RemoteEpisode { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }

        public TrackedDownload()
        {
            StatusMessages = new List<TrackedDownloadStatusMessage>();
        }

        public void SetStatusLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Warn)
            {
                Status = TrackedDownloadStatus.Warning;
            }

            if (logLevel >= LogLevel.Error)
            {
                Status = TrackedDownloadStatus.Error;
            }

            else Status = TrackedDownloadStatus.Ok;
        }
    }

    public enum TrackedDownloadState
    {
        Unknown,
        Downloading,
        Imported,
        DownloadFailed,
        Removed
    }

    public enum TrackedDownloadStatus
    {
        Ok,
        Warning,
        Error
    }
}
