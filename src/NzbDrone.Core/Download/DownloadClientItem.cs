using System;
using System.Diagnostics;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Download
{
    [DebuggerDisplay("{DownloadClient}:{Title}")]
    public class DownloadClientItem
    {
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }

        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public double? SeedRatio { get; set; }

        public OsPath OutputPath { get; set; }
        public string Message { get; set; }

        public DownloadItemStatus Status { get; set; }
        public bool IsEncrypted { get; set; }

        public bool CanMoveFiles { get; set; }
        public bool CanBeRemoved { get; set; }

        public bool Removed { get; set; }
    }
}
