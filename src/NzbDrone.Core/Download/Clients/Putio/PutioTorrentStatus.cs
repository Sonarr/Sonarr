namespace NzbDrone.Core.Download.Clients.Putio
{
    public static class PutioTorrentStatus
    {
        public static readonly string Waiting = "WAITING";
        public static readonly string PrepareDownload = "PREPARING_DOWNLOAD";
        public static readonly string Completed = "COMPLETED";
        public static readonly string Completing = "COMPLETING";
        public static readonly string Downloading = "DOWNLOADING";
        public static readonly string Error = "ERROR";
        public static readonly string InQueue = "IN_QUEUE";
        public static readonly string Seeding = "SEEDING";
    }
}
