using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class ResponsePorlaPresetsList
    {
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaPreset> Presets { get; set; }
    }

    // TODO: Figure out all the fields in here.
    public sealed class PorlaPreset : object
    {
        [JsonProperty("save_path", NullValueHandling = NullValueHandling.Ignore)]
        public string SavePath { get; set; }
    }
}
