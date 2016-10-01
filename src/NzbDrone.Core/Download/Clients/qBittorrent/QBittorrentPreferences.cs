using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // qbittorrent settings from the list returned by /query/preferences
    public class QBittorrentPreferences
    {
        [JsonProperty(PropertyName = "save_path")]
        public string SavePath { get; set; } // Default save path for torrents, separated by slashes

        [JsonProperty(PropertyName = "max_ratio_enabled")]
        public bool MaxRatioEnabled { get; set; } // True if share ratio limit is enabled

        [JsonProperty(PropertyName = "max_ratio")]
        public float MaxRatio { get; set; } // Get the global share ratio limit

        [JsonProperty(PropertyName = "max_ratio_act")]
        public bool RemoveOnMaxRatio { get; set; } // Action performed when a torrent reaches the maximum share ratio. [false = pause, true = remove]
    }
}
