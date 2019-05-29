namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionTorrent
    {
        public int Id { get; set; }
        public string HashString { get; set; }
        public string Name { get; set; }
        public string DownloadDir { get; set; }
        public long TotalSize { get; set; }
        public long LeftUntilDone { get; set; }
        public bool IsFinished { get; set; }
        public int Eta { get; set; }
        public TransmissionTorrentStatus Status { get; set; }
        public int SecondsDownloading { get; set; }
        public int SecondsSeeding { get; set; }
        public string ErrorString { get; set; }
        public long DownloadedEver { get; set; }
        public long UploadedEver { get; set; }
        public double SeedRatioLimit { get; set; }
        public int SeedRatioMode { get; set; }
        public long SeedIdleLimit { get; set; }
        public int SeedIdleMode { get; set; }
        public int FileCount { get; set; }
    }
}
