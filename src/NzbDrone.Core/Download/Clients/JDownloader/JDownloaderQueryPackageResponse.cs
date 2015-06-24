namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderQueryPackageResponse
    {
        public JDownloaderQueryPackageItem[] data { get; set; }
        public int rid { get; set; }
    }
}
