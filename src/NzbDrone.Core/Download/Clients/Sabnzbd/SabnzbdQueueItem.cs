using System;
using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdQueueItem
    {
        public SabnzbdDownloadStatus Status { get; set; }
        public int Index { get; set; }

        [JsonConverter(typeof(SabnzbdQueueTimeConverter))]
        public TimeSpan Timeleft { get; set; }

        [JsonProperty(PropertyName = "mb")]
        public decimal Size { get; set; }

        [JsonProperty(PropertyName = "filename")]
        public string Title { get; set; }

        [JsonConverter(typeof(SabnzbdPriorityTypeConverter))]
        public SabnzbdPriority Priority { get; set; }

        [JsonProperty(PropertyName = "cat")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "mbleft")]
        public decimal Sizeleft { get; set; }

        public int Percentage { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }
    }
}
