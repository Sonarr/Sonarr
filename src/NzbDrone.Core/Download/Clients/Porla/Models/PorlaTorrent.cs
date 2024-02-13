using System.Collections.Generic;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class PorlaTorrent
    {
        public LibTorrentInfoHash InfoHash { get; set; }

        public object[] AsParam() {
            return [InfoHash.Hash, null];
        }

        public object[] AsParams() {
            return new [ "info_hash", [InfoHash.Hash, null] ];
        }
    }
}
