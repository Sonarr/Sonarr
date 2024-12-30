using Newtonsoft.Json;

namespace Workarr.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexRetryResponse
    {
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }
    }
}
