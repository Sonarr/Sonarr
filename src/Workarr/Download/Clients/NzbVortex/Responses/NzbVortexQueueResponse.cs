using Newtonsoft.Json;

namespace Workarr.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexQueueResponse : NzbVortexResponseBase
    {
        [JsonProperty(PropertyName = "nzbs")]
        public List<NzbVortexQueueItem> Items { get; set; }
    }
}
