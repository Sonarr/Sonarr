namespace NzbDrone.Core.Download.Clients.Deluge
{
    class DelugeTorrentStatus
    {
        public const string Paused = "Paused";
        public const string Queued = "Queued";
        public const string Downloading = "Downloading";
        public const string Seeding = "Seeding";
        public const string Checking = "Checking";
        public const string Error = "Error";
    }
}
