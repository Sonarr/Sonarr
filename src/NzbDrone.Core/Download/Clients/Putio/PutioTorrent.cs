using System;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioTorrent
    {
        public int Id { get; set; }

        public string HashString { get; set; }

        public string Name { get; set; }

        public string DownloadDir { get; set; }

        public long TotalSize { get; set; }

        public long LeftUntilDone { get; set; }

        public bool IsFinished { get; set; }

        public int Eta { get; set; }

        public PutioTorrentStatus Status { get; set; }

        public int SecondsDownloading { get; set; }

        public string ErrorString { get; set; }
    }
}
