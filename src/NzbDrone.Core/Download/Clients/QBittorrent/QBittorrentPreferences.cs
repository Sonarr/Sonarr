using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public enum QBittorrentMaxRatioAction
    {
        Pause = 0,
        Remove = 1,
        EnableSuperSeeding = 2,
        DeleteFiles = 3
    }

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

        [JsonProperty(PropertyName = "max_inactive_seeding_time_enabled")]
        public bool MaxInactiveSeedingTimeEnabled { get; set; } // True if share inactive time limit is enabled

        [JsonProperty(PropertyName = "max_inactive_seeding_time")]
        public long MaxInactiveSeedingTime { get; set; } // Get the global share inactive time limit in minutes

        [JsonProperty(PropertyName = "max_ratio_act")]
        public QBittorrentMaxRatioAction MaxRatioAction { get; set; } // Action performed when a torrent reaches the maximum share ratio.

        [JsonProperty(PropertyName = "queueing_enabled")]
        public bool QueueingEnabled { get; set; } = true;

        [JsonProperty(PropertyName = "dht")]
        public bool DhtEnabled { get; set; } // DHT enabled (needed for more peers and magnet downloads)
    }
}
