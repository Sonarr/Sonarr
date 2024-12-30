using Newtonsoft.Json;

namespace Workarr.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdRetryResponse
    {
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }
    }
}
