using System;
using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.Sabnzbd.JsonConverters;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabQueueItem
    {
        public string Status { get; set; }
        public int Index { get; set; }

        [JsonConverter(typeof(SabnzbdQueueTimeConverter))]
        public TimeSpan Timeleft { get; set; }

        [JsonProperty(PropertyName = "mb")]
        public decimal Size { get; set; }

        private string _title;

        [JsonProperty(PropertyName = "filename")]
        public string Title { get; set; }

        [JsonConverter(typeof(SabnzbdPriorityTypeConverter))]
        public SabPriorityType Priority { get; set; }

        [JsonProperty(PropertyName = "cat")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "mbleft")]
        public decimal SizeLeft { get; set; }

        public int Percentage { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }
    }
}
