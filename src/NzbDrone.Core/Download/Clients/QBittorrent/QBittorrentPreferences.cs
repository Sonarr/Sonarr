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

        [JsonProperty(PropertyName = "max_seeding_time_enabled")]
        public bool MaxSeedingTimeEnabled { get; set; } // True if share time limit is enabled

        [JsonProperty(PropertyName = "max_seeding_time")]
        public long MaxSeedingTime { get; set; } // Get the global share time limit in minutes

        [JsonProperty(PropertyName = "max_ratio_act")]
        public bool RemoveOnMaxRatio { get; set; } // Action performed when a torrent reaches the maximum share ratio. [false = pause, true = remove]

        [JsonProperty(PropertyName = "queueing_enabled")]
        public bool QueueingEnabled { get; set; } = true;

        [JsonProperty(PropertyName = "dht")]
        public bool DhtEnabled { get; set; } // DHT enabled (needed for more peers and magnet downloads)
    }
}
