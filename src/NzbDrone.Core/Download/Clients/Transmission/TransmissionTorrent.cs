using System;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionTorrent
    {
        public Int32 Id { get; set; }

        public String HashString { get; set; }

        public String Name { get; set; }

        public String DownloadDir { get; set; }

        public Int64 TotalSize { get; set; }

        public Int64 LeftUntilDone { get; set; }

        public Boolean IsFinished { get; set; }

        public Int32 Eta { get; set; }

        public TransmissionTorrentStatus Status { get; set; }

        public Int32 SecondsDownloading { get; set; }

        public String ErrorString { get; set; }
    }
}
