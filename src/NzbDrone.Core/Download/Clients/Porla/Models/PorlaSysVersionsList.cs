using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> The data type for the <em>porla</em> field in the <em>sys.versions</em> response </summary>
    public class PorlaSysVersionsPorla
    {
        [JsonProperty("branch", NullValueHandling = NullValueHandling.Ignore)]
        public string Branch { get; set; }

        [JsonProperty("commitish", NullValueHandling = NullValueHandling.Ignore)]
        public string Commitish { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    /// <summary> The response for the <em>sys.versions</em> call to porla </summary>
    public class PorlaSysVersions
    {
        [JsonProperty("boost", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Boost { get; set; }

        [JsonProperty("libtorrent", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> LibTorrent { get; set; }

        [JsonProperty("nlohmann_json", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> NlohmannJson { get; set; }

        [JsonProperty("openssl", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> OpenSSL { get; set; }

        [JsonProperty("porla", NullValueHandling = NullValueHandling.Ignore)]
        public PorlaSysVersionsPorla Porla { get; set; }

        [JsonProperty("sqlite", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Sqlite { get; set; }

        [JsonProperty("tomlplusplus", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> TOMLPlusPlus { get; set; }
    }
}
