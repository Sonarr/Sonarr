using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeUpdateUIResult
    {
        public Dictionary<string, object> Stats { get; set; }
        public bool Connected { get; set; }
        public Dictionary<string, DelugeTorrent> Torrents { get; set; }
    }
}
