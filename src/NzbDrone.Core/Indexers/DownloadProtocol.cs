using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers
{
    public enum DownloadProtocol
    {
        Unknown = 0,
        Usenet = 1,
        Torrent = 2
    }
}
