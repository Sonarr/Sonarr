using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Profiles.Delay
{
    public class DelayProfileResource : RestResource
    {
        public bool EnableUsenet { get; set; }
        public bool EnableTorrent { get; set; }
        public DownloadProtocol PreferredProtocol { get; set; }
        public int UsenetDelay { get; set; }
        public int TorrentDelay { get; set; }
        public int Order { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}
