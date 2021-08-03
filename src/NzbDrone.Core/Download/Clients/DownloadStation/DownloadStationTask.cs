using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class DownloadStationTask
    {
        public string Username { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// /// Possible values are: BT, NZB, http, ftp, eMule and https
        /// </summary>
        public string Type { get; set; }

        [JsonProperty(PropertyName = "status_extra")]
        public Dictionary<string, string> StatusExtra { get; set; }

        [JsonConverter(typeof(UnderscoreStringEnumConverter), DownloadStationTaskStatus.Unknown)]
        public DownloadStationTaskStatus Status { get; set; }

        public DownloadStationTaskAdditional Additional { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public enum DownloadStationTaskType
    {
        BT,
        NZB,
        http,
        ftp,
        eMule,
        https
    }

    public enum DownloadStationTaskStatus
    {
        Unknown,
        Waiting,
        Downloading,
        Paused,
        Finishing,
        Finished,
        HashChecking,
        Seeding,
        FilehostingWaiting,
        Extracting,
        Error,
        CaptchaNeeded
    }

    public enum DownloadStationPriority
    {
        Auto,
        Low,
        Normal,
        High
    }
}
