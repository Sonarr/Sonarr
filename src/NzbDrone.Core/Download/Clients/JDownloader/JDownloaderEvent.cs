namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderEvent
    {
        public string eventName { get; set; }
        public JDownloaderEventData eventData { get; set; }
    }
}
