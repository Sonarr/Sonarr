using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrents
    {
        public Dictionary<Int32, BroadcastheNetTorrent> Torrents { get; set; }
        public Int32 Results { get; set; }
    }
}
