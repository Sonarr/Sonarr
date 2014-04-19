using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download
{
    public enum DownloadItemStatus
    {
        Queued = 0,
        Paused = 1,
        Downloading = 2,
        Completed = 3,
        Failed = 4
    }
}
