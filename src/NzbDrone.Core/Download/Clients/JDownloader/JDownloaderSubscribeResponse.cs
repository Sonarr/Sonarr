using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public class JDownloaderSubscribeResponse
    {
        public int subscriptionid { get; set; }
        public bool subscribed { get; set; }
        public List<string> subscriptions { get; set; }
        public List<string> exclusions { get; set; }
        public long maxPolltimeout { get; set; }
        public long maxKeepalive { get; set; }
    }
}
