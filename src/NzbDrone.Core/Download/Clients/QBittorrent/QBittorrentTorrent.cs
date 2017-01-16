using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // torrent properties from the list returned by /query/torrents
    public class QBittorrentTorrent
    {
        public string Hash { get; set; } // Torrent hash

        public string Name { get; set; } // Torrent name

        public long Size { get; set; } // Torrent size (bytes)

        public double Progress { get; set; } // Torrent progress (%/100)

        public ulong Eta { get; set; } // Torrent ETA (seconds)

        public string State { get; set; } // Torrent state. See possible values here below

        public string Label { get; set; } // Label of the torrent
        public string Category { get; set; } // Category of the torrent (3.3.5+)

        [JsonProperty(PropertyName = "save_path")]
        public string SavePath { get; set; } // Torrent save path

        public float Ratio { get; set; } // Torrent share ratio
    }
}
