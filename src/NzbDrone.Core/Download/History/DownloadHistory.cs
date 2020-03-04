using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Download.History
{
    public class DownloadHistory : ModelBase
    {
        public DownloadHistoryEventType EventType { get; set; }
        public int SeriesId { get; set; }
        public string DownloadId { get; set; }
        public string SourceTitle { get; set; }
        public DateTime Date { get; set; }
    }

    public enum DownloadHistoryEventType
    {
        DownloadGrabbed = 1,
        DownloadImported = 2,
        DownloadFailed = 3,
        DownloadIgnored = 4,
        FileImported = 5
    }
}
