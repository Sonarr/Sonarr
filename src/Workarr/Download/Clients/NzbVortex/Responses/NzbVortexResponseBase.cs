using Newtonsoft.Json;
using Workarr.Download.Clients.NzbVortex.JsonConverters;

namespace Workarr.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexResultTypeConverter))]
        public NzbVortexResultType Result { get; set; }
    }
}
