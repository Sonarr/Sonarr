using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class ResponsePorlaSessionList
    {
        [JsonProperty("sessions", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaSession> Sessions { get; set; }
    }

    public class PorlaSession
    {
        [JsonProperty("is_dht_running", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDHTRunning { get; set; }

        [JsonProperty("is_listening", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsListening { get; set; }

        [JsonProperty("is_paused", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPaused { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("torrents_total", NullValueHandling = NullValueHandling.Ignore)]
        public long TorrentsTotal { get; set; }
    }
}
