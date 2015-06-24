namespace NzbDrone.Core.Download.Clients.JDownloader
{
    internal class JDownloaderError
    {
        public string data { get; set; }
        public string src { get; set; }
        public string type { get; set; }
    }
}