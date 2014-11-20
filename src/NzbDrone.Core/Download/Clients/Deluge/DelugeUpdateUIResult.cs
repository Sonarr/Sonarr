using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeUpdateUIResult
    {
        public Dictionary<String, Object> Stats { get; set; }
        public Boolean Connected { get; set; }
        public Dictionary<String, DelugeTorrent> Torrents { get; set; }
    }
}
