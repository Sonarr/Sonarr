namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderQueryPackageItem
    {
        public string name { get; set; }
        public string comment { get; set; }
        public string status { get; set; }
        public int offlineCount { get; set; }
        public int childCount { get; set; }
        public int onlineCount { get; set; }
        public int tempUnknownCount { get; set; }
        public int unknownCount { get; set; }
        public long uuid { get; set; }
        public bool enabled { get; set; }
        public long bytesTotal { get; set; }
        public string[] hosts { get; set; }
        public string saveTo { get; set; }
        public string speed { get; set; }
        public bool finished { get; set; }
        public bool running { get; set; }
        public string eta { get; set; }
        public long bytesLoaded { get; set; }

        

    }
}