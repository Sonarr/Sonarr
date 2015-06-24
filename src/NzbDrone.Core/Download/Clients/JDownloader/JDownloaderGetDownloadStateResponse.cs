namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderGetDownloadStateResponse
    {
        public string data { get; set; }
        public int rid { get; set; }

        public JDownloaderState State
        {
            get
            {
                if (data == "STOPPED_STATE")
                    return JDownloaderState.Stopped;
                else if (data == "RUNNING")
                    return JDownloaderState.Running;
                else
                    return JDownloaderState.None;
            }
        }
    }
}