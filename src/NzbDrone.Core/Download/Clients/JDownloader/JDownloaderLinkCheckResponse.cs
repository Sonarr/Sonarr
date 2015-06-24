namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderLinkCheckResponse
    {
        public string name { get; set; }
        public string status { get; set; }
        public string comment { get; set; }
        public long packageUUID { get; set; }
        public long uuid { get; set; }
        public string host { get; set; }
        public string url { get; set; }
        public string availability { get; set; }
        public long bytesTotal { get; set; }
        public long bytesLoaded { get; set; }
        public bool finished { get; set; }
        public bool enabled { get; set; }
        public long speed { get; set; }
        public long eta { get; set; }
        
        public string variantID { get; set; }
        public string variantName { get; set; }
        public bool variants { get; set; }
        public string priority { get; set; }

        public bool IsOnline
        {
            get
            {
                return availability == "ONLINE";
            }
        }
    }
}
