using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetTorrents
    {
        public Dictionary<Int32, BroadcastheNetTorrent> Torrents { get; set; }
        public Int32 Results { get; set; }
    }
}
