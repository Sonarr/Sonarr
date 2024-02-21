using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public class PorlaSession
    {
        [JsonProperty("is_dht_running", NullValueHandling = NullValueHandling.Ignore)]
        public bool is_dht_running { get; set; }

        [JsonProperty("is_listening", NullValueHandling = NullValueHandling.Ignore)]
        public bool is_listening { get; set; }

        [JsonProperty("is_paused", NullValueHandling = NullValueHandling.Ignore)]
        public bool is_paused { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string name { get; set; }

        [JsonProperty("torrents_total", NullValueHandling = NullValueHandling.Ignore)]
        public long torrents_total { get; set; }
    }
}