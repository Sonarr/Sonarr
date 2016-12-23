namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetGlobalStatus
    {
        public uint RemainingSizeLo { get; set; }
        public uint RemainingSizeHi { get; set; }
        public uint DownloadedSizeLo { get; set; }
        public uint DownloadedSizeHi { get; set; }
        public int DownloadRate { get; set; }
        public int AverageDownloadRate { get; set; }
        public int DownloadLimit { get; set; }
        public bool DownloadPaused { get; set; }
    }
}
