using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NzbDrone.Core.Helpers;

namespace NzbDrone.Core.Model.Sabnzbd
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
