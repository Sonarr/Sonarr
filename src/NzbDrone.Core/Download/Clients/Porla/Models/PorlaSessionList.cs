using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> Implementation of the <em>session</em> field in the response from <a href="https://github.com/porla/porla/blob/v0.37.0/src/methods/sessions/sessionslist.cpp">sessionslist.cpp</a> data type </summary>
    public sealed class ResponsePorlaSessionList
    {
        [JsonProperty("sessions", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaSession> Sessions { get; set; }
    }

    /// <summary> Implementation of the session data type from the response <a href="https://github.com/porla/porla/blob/v0.37.0/src/methods/sessions/sessionslist.cpp">sessionslist.cpp</a></summary>
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
        public int TorrentsTotal { get; set; }
    }
}
