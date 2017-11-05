namespace NzbDrone.Core.Download.Clients.Hadouken.Models
{
    public sealed class HadoukenTorrent
    {
        public string InfoHash { get; set; }
        public double Progress { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string SavePath { get; set; }
        public HadoukenTorrentState State { get; set; }
        public bool IsFinished { get; set; }
        public bool IsPaused { get; set; }
        public bool IsSeeding { get; set; }
        public long TotalSize { get; set; }
        public long DownloadedBytes { get; set; }
        public long UploadedBytes { get; set; }
        public long DownloadRate { get; set; }
        public string Error { get; set; }
    }
}
