using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.BroadcasTheNet
{
    public class BroadcasTheNetTorrents
    {
        public Dictionary<int, BroadcasTheNetTorrent> Torrents { get; set; }
        public int Results { get; set; }
    }
}
