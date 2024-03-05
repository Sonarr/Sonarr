using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class ResponsePorlaPresetsList
    {
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaPreset> Presets { get; set; }
    }

    public sealed class PorlaPreset : object
    {
    }
}
