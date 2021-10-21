namespace NzbDrone.Core.Indexers.Tribler
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public class GetTriblerSettingsResponse
    {
        [Newtonsoft.Json.JsonProperty("settings", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Settings Settings { get; set; }
    }


    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Settings
    {
        [Newtonsoft.Json.JsonProperty("general", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public General General { get; set; }

        [Newtonsoft.Json.JsonProperty("tunnel_community", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Tunnel_community Tunnel_community { get; set; }

        [Newtonsoft.Json.JsonProperty("dht", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dht Dht { get; set; }

        [Newtonsoft.Json.JsonProperty("download_defaults", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Download_defaults Download_defaults { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class General
    {
        [Newtonsoft.Json.JsonProperty("log_dir", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Log_dir { get; set; }

        [Newtonsoft.Json.JsonProperty("version", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("version_checker_enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Version_checker_enabled { get; set; }

        [Newtonsoft.Json.JsonProperty("testnet", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Testnet { get; set; }
    }


    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Tunnel_community
    {
        [Newtonsoft.Json.JsonProperty("exitnode_enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Exitnode_enabled { get; set; }

        [Newtonsoft.Json.JsonProperty("enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [Newtonsoft.Json.JsonProperty("random_slots", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Random_slots { get; set; }

        [Newtonsoft.Json.JsonProperty("competing_slots", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Competing_slots { get; set; }

        [Newtonsoft.Json.JsonProperty("min_circuits", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Min_Circuits { get; set; }

        [Newtonsoft.Json.JsonProperty("max_circuits", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Max_Circuits { get; set; }

        [Newtonsoft.Json.JsonProperty("testnet", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Testnet { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Dht
    {
        [Newtonsoft.Json.JsonProperty("enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }


    }



    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Download_defaults
    {
        [Newtonsoft.Json.JsonProperty("anonymity_enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Anonymity_enabled { get; set; }

        [Newtonsoft.Json.JsonProperty("number_hops", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Number_hops { get; set; }

        [Newtonsoft.Json.JsonProperty("safeseeding_enabled", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Safeseeding_enabled { get; set; }

        [Newtonsoft.Json.JsonProperty("saveas", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Saveas { get; set; }

        [Newtonsoft.Json.JsonProperty("seeding_mode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public Download_defaultsSeeding_mode? Seeding_mode { get; set; }

        /// <summary>Seeding ratio download/upload</summary>
        [Newtonsoft.Json.JsonProperty("seeding_ratio", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double? Seeding_ratio { get; set; }

        /// <summary>Seeding time in seconds</summary>
        [Newtonsoft.Json.JsonProperty("seeding_time", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double? Seeding_time { get; set; }


    }


    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum Download_defaultsSeeding_mode
    {
        [System.Runtime.Serialization.EnumMember(Value = @"ratio")]
        Ratio = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"forever")]
        Forever = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"time")]
        Time = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"never")]
        Never = 3,

    }
}
