namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderGetStateResponse
    {
        public JDownloaderEvent[] data { get; set; }
        public int rid { get; set; }
    }
}
