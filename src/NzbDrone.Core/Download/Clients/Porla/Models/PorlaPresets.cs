using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class ResponsePorlaPresetsList
    {
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public List<PorlaPreset> presets { get; set; }
    }

    public sealed class PorlaPreset : IReadOnlyDictionary<string, object>
    {

    }
}
