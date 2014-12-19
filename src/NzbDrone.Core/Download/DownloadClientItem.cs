using System;
using System.Diagnostics;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    [DebuggerDisplay("{DownloadClient}:{Title}")]
    public class DownloadClientItem
    {
        public String DownloadClient { get; set; }
        public String DownloadId { get; set; }
        public String Category { get; set; }
        public String Title { get; set; }

        public Int64 TotalSize { get; set; }
        public Int64 RemainingSize { get; set; }
        public TimeSpan? RemainingTime { get; set; }

        public OsPath OutputPath { get; set; }
        public String Message { get; set; }

        public DownloadItemStatus Status { get; set; }
        public Boolean IsEncrypted { get; set; }
        public Boolean IsReadOnly { get; set; }

        public bool Removed { get; set; }
    }
}
