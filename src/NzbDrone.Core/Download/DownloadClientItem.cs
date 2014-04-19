using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download
{
    public class DownloadClientItem
    {
        public string DownloadClient { get; set; }
        public string DownloadClientId { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }

        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public TimeSpan DownloadTime { get; set; }
        public TimeSpan RemainingTime { get; set; }

        public string OutputPath { get; set; }
        public string Message { get; set; }

        public DownloadItemStatus Status { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsReadOnly { get; set; }
        public RemoteEpisode RemoteEpisode { get; set; }
    }
}
