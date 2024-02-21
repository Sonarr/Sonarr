using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{

    public class PorlaSysVersionsPorla
    {
        [JsonProperty("branch", NullValueHandling = NullValueHandling.Ignore)]
        public string branch { get; set; }

        [JsonProperty("commitish", NullValueHandling = NullValueHandling.Ignore)]
        public string commitish { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string version { get; set; }
    }

    public class PorlaSysVersions
    {
        [JsonProperty("boost", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> boost { get; set; }

        [JsonProperty("libtorrent", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> libtorrent { get; set; }

        [JsonProperty("nlohmann_json", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> nlohmann_json { get; set; }

        [JsonProperty("openssl", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> openssl { get; set; }

        [JsonProperty("porla", NullValueHandling = NullValueHandling.Ignore)]
        public PorlaSysVersionsPorla porla { get; set; }

        [JsonProperty("sqlite", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> sqlite { get; set; }

        [JsonProperty("tomlplusplus", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> tomlplusplus { get; set; }
    }
}