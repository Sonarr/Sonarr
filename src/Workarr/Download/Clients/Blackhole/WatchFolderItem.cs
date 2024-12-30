﻿using Workarr.Disk;

namespace Workarr.Download.Clients.Blackhole
{
    public class WatchFolderItem
    {
        public string DownloadId { get; set; }
        public string Title { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public OsPath OutputPath { get; set; }
        public DownloadItemStatus Status { get; set; }

        public DateTime LastChanged { get; set; }
        public string Hash { get; set; }
    }
}
