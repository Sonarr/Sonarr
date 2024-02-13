using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.LibTorrent.Models
{
    public sealed class LibTorrentInfoHash
    {
        public string? Hash { get; set; }
        public string? Status { get; set; }
    }
}
