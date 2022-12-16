using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Profiles.Delay
{
    public class DelayProfile : ModelBase
    {
        public bool EnableUsenet { get; set; }
        public bool EnableTorrent { get; set; }
        public DownloadProtocol PreferredProtocol { get; set; }
        public int UsenetDelay { get; set; }
        public int TorrentDelay { get; set; }
        public int Order { get; set; }
        public bool BypassIfHighestQuality { get; set; }
        public bool BypassIfAboveCustomFormatScore { get; set; }
        public int MinimumCustomFormatScore { get; set; }
        public HashSet<int> Tags { get; set; }

        public DelayProfile()
        {
            Tags = new HashSet<int>();
        }

        public int GetProtocolDelay(DownloadProtocol protocol)
        {
            return protocol == DownloadProtocol.Torrent ? TorrentDelay : UsenetDelay;
        }
    }
}
