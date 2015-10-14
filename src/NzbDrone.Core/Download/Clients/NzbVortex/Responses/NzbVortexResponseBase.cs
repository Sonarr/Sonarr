using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.NzbVortex.JsonConverters;

namespace NzbDrone.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexResponseBase
    {
        [JsonConverter(typeof(NzbVortexResultTypeConverter))]
        public NzbVortexResultType Result { get; set; }
    }
}
