using System;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model;

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
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                ParseResult = Parser.ParseTitle(value.Replace("DUPLICATE / ", String.Empty));
            }
        }

        [JsonConverter(typeof(SabnzbdPriorityTypeConverter))]
        public SabPriorityType Priority { get; set; }

        [JsonProperty(PropertyName = "cat")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "mbleft")]
        public decimal SizeLeft { get; set; }

        public int Percentage { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }

        public EpisodeParseResult ParseResult { private set; get; }
    }
}
