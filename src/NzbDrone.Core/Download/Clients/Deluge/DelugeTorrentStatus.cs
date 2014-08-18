using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    class DelugeTorrentStatus
    {
        public const String Paused = "Paused";
        public const String Queued = "Queued";
        public const String Downloading = "Downloading";
        public const String Seeding = "Seeding";
        public const String Checking = "Checking";
        public const String Error = "Error";
    }
}
