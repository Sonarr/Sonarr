using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrentTorrentCache
    {
        public string CacheID { get; set; }
        
        public List<UTorrentTorrent> Torrents { get; set; }
    }
}
