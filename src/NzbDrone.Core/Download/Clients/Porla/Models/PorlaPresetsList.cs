using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> Porla Session Extention Methods </summary>
    public static class PorlaPresetsExtentions
    {
        /// <summary> Gets the spesified preset values merged with the values from the default preset </summary>
        public static PorlaPreset GetEffective(this ReadOnlyDictionary<string, PorlaPreset> presets, string preset)
        {
            if (presets == null)
            {
                return new PorlaPreset();
            }

            var defaultExist  = presets.ContainsKey("default");
            var presetExist   = presets.ContainsKey(preset ?? "");
            var defaultPreset = presets.GetValueOrDefault("default");
            var activePreset  = presets.GetValueOrDefault(preset ?? "");
            if (defaultExist && presetExist)
            {
                // TODO: There has to be a better way to merge these
                return new PorlaPreset()
                {
                    Category        = activePreset.Category         ?? defaultPreset.Category,
                    MaxConnections  = activePreset.MaxConnections   ?? defaultPreset.MaxConnections,
                    MaxUploads      = activePreset.MaxUploads       ?? defaultPreset.MaxUploads,
                    SavePath        = activePreset.SavePath         ?? defaultPreset.SavePath,
                    Session         = activePreset.Session          ?? defaultPreset.Session,
                    Tags            = activePreset.Tags             ?? defaultPreset.Tags,
                    UploadLimit     = activePreset.UploadLimit      ?? defaultPreset.UploadLimit
                };
            }

            // default doesn't exist
            if (presetExist)
            {
                return activePreset;
            }

            // active doesn't exist
            if (defaultExist)
            {
                return defaultPreset;
            }

            // neither exists.
            return new PorlaPreset();
        }
    }

    /// <summary> Implementation of the list presets response data type from <a href="https://github.com/porla/porla/blob/v0.37.0/src/lua/packages/presets.cpp">presets.cpp</a></summary>
    public sealed class ResponsePorlaPresetsList
    {
        [JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyDictionary<string, PorlaPreset> Presets { get; set; }
    }

    /// <summary> Implementation of the <em>preset</em> data type in the response data from <a href="https://github.com/porla/porla/blob/v0.37.0/src/lua/packages/presets.cpp">presets.cpp</a></summary>
    public sealed class PorlaPreset : object
    {
        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("max_connections", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxConnections { get; set; }

        [JsonProperty("max_uploads", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxUploads { get; set; }

        [JsonProperty("save_path", NullValueHandling = NullValueHandling.Ignore)]
        public string SavePath { get; set; }

        [JsonProperty("session", NullValueHandling = NullValueHandling.Ignore)]
        public string Session { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<string> Tags { get; set; }

        [JsonProperty("upload_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UploadLimit { get; set; }
    }
}
