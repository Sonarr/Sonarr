using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexRetryResponse
    {
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "nzo_id")]
        public string Id { get; set; }
    }
}
