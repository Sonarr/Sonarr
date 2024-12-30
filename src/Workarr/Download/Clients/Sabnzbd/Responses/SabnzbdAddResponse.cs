using Newtonsoft.Json;

namespace Workarr.Download.Clients.Sabnzbd.Responses
{
    public class SabnzbdAddResponse
    {
        public SabnzbdAddResponse()
        {
            Ids = new List<string>();
        }

        public bool Status { get; set; }

        [JsonProperty(PropertyName = "nzo_ids")]
        public List<string> Ids { get; set; }
    }
}
