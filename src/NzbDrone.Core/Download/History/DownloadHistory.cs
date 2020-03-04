using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.History
{
    public class DownloadHistory : ModelBase
    {
        public DownloadHistoryEventType EventType { get; set; }
        public int SeriesId { get; set; }
        public string DownloadId { get; set; }
        public string SourceTitle { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public int IndexerId { get; set; }
        public int DownloadClientId { get; set; }
        public ReleaseInfo Release { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public DownloadHistory()
        {
            Data = new Dictionary<string, string>();
        }
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
