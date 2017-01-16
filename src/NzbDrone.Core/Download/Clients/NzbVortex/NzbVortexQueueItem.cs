namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexQueueItem
    {
        public int Id { get; set; }
        public string UiTitle { get; set; }
        public string DestinationPath { get; set; }
        public string NzbFilename { get; set; }
        public bool IsPaused { get; set; }
        public NzbVortexStateType State { get; set; }
        public string StatusText { get; set; }
        public int TransferedSpeed { get; set; }
        public double Progress { get; set; }
        public long DownloadedSize { get; set; }
        public long TotalDownloadSize { get; set; }
        public long PostDate { get; set; }
        public int TotalArticleCount { get; set; }
        public int FailedArticleCount { get; set; }
        public string GroupUUID { get; set; }
        public string AddUUID { get; set; }
        public string GroupName { get; set; }
    }
}
