using System;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    [Flags]
    public enum UTorrentTorrentStatus
    {
        Started = 1,
        Checking = 2,
        StartAfterCheck = 4,
        Checked = 8,
        Error = 16,
        Paused = 32,
        Queued = 64,
        Loaded = 128
    }
}
