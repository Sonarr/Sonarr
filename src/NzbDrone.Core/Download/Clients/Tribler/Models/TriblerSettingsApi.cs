using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class GetTriblerSettingsResponse
    {
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        [JsonProperty("general", NullValueHandling = NullValueHandling.Ignore)]
        public General General { get; set; }

        [JsonProperty("tunnel_community", NullValueHandling = NullValueHandling.Ignore)]
        public TunnelCommunity TunnelCommunity { get; set; }

        [JsonProperty("dht", NullValueHandling = NullValueHandling.Ignore)]
        public Dht Dht { get; set; }

        [JsonProperty("download_defaults", NullValueHandling = NullValueHandling.Ignore)]
        public DownloadDefaults DownloadDefaults { get; set; }
    }

    public class General
    {
        [JsonProperty("log_dir", NullValueHandling = NullValueHandling.Ignore)]
        public string LogDir { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("version_checker_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VersionCheckerEnabled { get; set; }

        [JsonProperty("testnet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TestNet { get; set; }
    }

    public class TunnelCommunity
    {
        [JsonProperty("exitnode_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ExitNodeEnabled { get; set; }

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("random_slots", NullValueHandling = NullValueHandling.Ignore)]
        public int? RandomSlots { get; set; }

        [JsonProperty("competing_slots", NullValueHandling = NullValueHandling.Ignore)]
        public int? CompetingSlots { get; set; }

        [JsonProperty("min_circuits", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinCircuits { get; set; }

        [JsonProperty("max_circuits", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxCircuits { get; set; }

        [JsonProperty("testnet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TestNet { get; set; }
    }

    public class Dht
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class DownloadDefaults
    {
        [JsonProperty("anonymity_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AnonymityEnabled { get; set; }

        [JsonProperty("number_hops", NullValueHandling = NullValueHandling.Ignore)]
        public int? NumberHops { get; set; }

        [JsonProperty("safeseeding_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SafeSeedingEnabled { get; set; }

        [JsonProperty("saveas", NullValueHandling = NullValueHandling.Ignore)]
        public string SaveAS { get; set; }

        [JsonProperty("seeding_mode", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadDefaultsSeedingMode? SeedingMode { get; set; }

        [JsonProperty("seeding_ratio", NullValueHandling = NullValueHandling.Ignore)]
        public double? SeedingRatio { get; set; }

        [JsonProperty("seeding_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? SeedingTime { get; set; }
    }

    public enum DownloadDefaultsSeedingMode
    {
        [EnumMember(Value = @"ratio")]
        Ratio = 0,

        [EnumMember(Value = @"forever")]
        Forever = 1,

        [EnumMember(Value = @"time")]
        Time = 2,

        [EnumMember(Value = @"never")]
        Never = 3,
    }
}
