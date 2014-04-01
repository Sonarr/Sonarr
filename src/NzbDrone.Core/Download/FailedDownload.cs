using System;

namespace NzbDrone.Core.Download
{
    public class FailedDownload
    {
        public HistoryItem DownloadClientHistoryItem { get; set; }
        public DateTime LastRetry { get; set; }
        public Int32 RetryCount { get; set; }
    }
}
