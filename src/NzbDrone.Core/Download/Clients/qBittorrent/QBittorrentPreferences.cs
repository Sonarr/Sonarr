using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // qbittorrent settings from the list returned by /query/preferences
    public class QBittorrentPreferences
    {
        [JsonProperty(PropertyName = "save_path")]
        public string SavePath { get; set; } // Default save path for torrents, separated by slashes
    }
}
