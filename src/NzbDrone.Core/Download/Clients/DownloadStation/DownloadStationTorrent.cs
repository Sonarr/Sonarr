using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTorrent
    {
        public string Username { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationTaskType Type { get; set; }

        [JsonProperty(PropertyName = "status_extra")]
        public Dictionary<string, string> StatusExtra { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStationTaskStatus Status { get; set; }

        public DownloadStationTorrentAdditional Additional { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public enum DownloadStationTaskType
    {
        BT, NZB, http, ftp, eMule
    }

    public enum DownloadStationTaskStatus
    {
        Waiting,
        Downloading,
        Paused,
        Finishing,
        Finished,
        HashChecking,
        Seeding,
        FileHostingWaiting,
        Extracting,
        Error
    }

    public enum DownloadStationPriority
    {
        Auto,
        Low,
        Normal,
        High
    }
}
