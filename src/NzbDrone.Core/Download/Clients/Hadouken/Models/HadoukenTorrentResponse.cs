using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Hadouken.Models
{
    public class HadoukenTorrentResponse
    {
        public object[][] Torrents { get; set; }
    }
}
