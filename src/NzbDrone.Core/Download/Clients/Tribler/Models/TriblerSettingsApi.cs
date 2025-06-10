using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Indexers.Tribler
{
    public class TriblerSettingsResponse
    {
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        [JsonProperty("api", NullValueHandling = NullValueHandling.Ignore)]
        public Api Api { get; set; }

        [JsonProperty("ipv8", NullValueHandling = NullValueHandling.Ignore)]
        public IpV8 IpV8 { get; set; }

        [JsonProperty("statistics", NullValueHandling = NullValueHandling.Ignore)]
        public bool Statistics { get; set; }

        [JsonProperty("content_discovery_community", NullValueHandling = NullValueHandling.Ignore)]
        public ContentDiscoveryCommunity ContentDiscoveryCommunity { get; set; }

        [JsonProperty("database", NullValueHandling = NullValueHandling.Ignore)]
        public Database Database { get; set; }

        [JsonProperty("dht_discovery", NullValueHandling = NullValueHandling.Ignore)]
        public DHTDiscovery DHTDiscovery { get; set; }

        [JsonProperty("knowledge_community", NullValueHandling = NullValueHandling.Ignore)]
        public KnowledgeCommunity KnowledgeCommunity { get; set; }

        [JsonProperty("libtorrent", NullValueHandling = NullValueHandling.Ignore)]
        public LibTorrent LibTorrent { get; set; }

        [JsonProperty("recommender", NullValueHandling = NullValueHandling.Ignore)]
        public Recommender Recommender { get; set; }

        [JsonProperty("rendezvous", NullValueHandling = NullValueHandling.Ignore)]
        public Rendezvous RecoRendezvousmmender { get; set; }

        [JsonProperty("torrent_checker", NullValueHandling = NullValueHandling.Ignore)]
        public TorrentChecker TorrentChecker { get; set; }

        [JsonProperty("tunnel_community", NullValueHandling = NullValueHandling.Ignore)]
        public TunnelCommunity TunnelCommunity { get; set; }

        [JsonProperty("versioning", NullValueHandling = NullValueHandling.Ignore)]
        public Versioning Versioning { get; set; }

        [JsonProperty("watch_folder", NullValueHandling = NullValueHandling.Ignore)]
        public WatchFolder WatchFolder { get; set; }

        [JsonProperty("state_dir", NullValueHandling = NullValueHandling.Ignore)]
        public string StateDir { get; set; }
        [JsonProperty("memory_db", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MemoryDB { get; set; }
    }

    public class Api
    {
        [JsonProperty("http_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool HttpEnabled { get; set; }

        [JsonProperty("http_port", NullValueHandling = NullValueHandling.Ignore)]
        public int HttpPort { get; set; }

        [JsonProperty("http_host", NullValueHandling = NullValueHandling.Ignore)]
        public string HttpHost { get; set; }

        [JsonProperty("https_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool HttpsEnabled { get; set; }

        [JsonProperty("https_port", NullValueHandling = NullValueHandling.Ignore)]
        public int HttpsPort { get; set; }

        [JsonProperty("https_host", NullValueHandling = NullValueHandling.Ignore)]
        public string HttpsHost { get; set; }

        [JsonProperty("https_certfile", NullValueHandling = NullValueHandling.Ignore)]
        public string HttpsCertFile { get; set; }

        [JsonProperty("http_port_running", NullValueHandling = NullValueHandling.Ignore)]
        public int HttpPortRunning { get; set; }

        [JsonProperty("https_port_running", NullValueHandling = NullValueHandling.Ignore)]
        public int HttpsPortRunning { get; set; }
    }

    public class IpV8
    {
        [JsonProperty("interfaces", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpV8Interfaces> Interfaces { get; set; }

        [JsonProperty("keys", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpV8Keys> Keys { get; set; }

        [JsonProperty("logger", NullValueHandling = NullValueHandling.Ignore)]
        public IpV8Logger Logger { get; set; }

        [JsonProperty("working_directory", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkingDirectory { get; set; }

        [JsonProperty("walker_interval", NullValueHandling = NullValueHandling.Ignore)]
        public double? WalkerInterval { get; set; }

        [JsonProperty("overlays", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpV8Overlay> Overlays { get; set; }
    }

    public class IpV8Interfaces
    {
        [JsonProperty("interface", NullValueHandling = NullValueHandling.Ignore)]
        public string Interface { get; set; }

        [JsonProperty("ip", NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public int? Port { get; set; }

        [JsonProperty("worker_threads", NullValueHandling = NullValueHandling.Ignore)]
        public int? WorkerThreads { get; set; }
    }

    public class IpV8Keys
    {
        [JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        [JsonProperty("generation", NullValueHandling = NullValueHandling.Ignore)]
        public string Generation { get; set; }

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public string File { get; set; }
    }

    public class IpV8Logger
    {
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public string Level { get; set; }
    }

    public class IpV8Overlay
    {
        [JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
        public string Class { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("walkers", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpV8OverlayWalker> Walkers { get; set; }

        // the content of this list is incomplete
        [JsonProperty("bootstrappers", NullValueHandling = NullValueHandling.Ignore)]
        public List<IpV8OverlayBootstrapper> Bootstrappers { get; set; }

        // missing initialize and on_start here, but they are empty so might not be needed.
    }

    public class IpV8OverlayWalker
    {
        [JsonProperty("strategy", NullValueHandling = NullValueHandling.Ignore)]
        public string Strategy { get; set; }

        [JsonProperty("peers", NullValueHandling = NullValueHandling.Ignore)]
        public int? Peers { get; set; }

        [JsonProperty("init", NullValueHandling = NullValueHandling.Ignore)]
        public IpV8OverlayWalkerInit Init { get; set; }
    }

    public class IpV8OverlayWalkerInit
    {
        [JsonProperty("timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int? Timeout { get; set; }

        [JsonProperty("sample_size", NullValueHandling = NullValueHandling.Ignore)]
        public int? SampleSize { get; set; }

        [JsonProperty("ping_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int? PingInterval { get; set; }

        [JsonProperty("inactive_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? InactiveTime { get; set; }

        [JsonProperty("drop_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? DropTime { get; set; }
    }

    public class IpV8OverlayBootstrapper
    {
        [JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
        public string Class { get; set; }

        // Data elements are missing here, do not try to store this to tribler
    }

    public class ContentDiscoveryCommunity
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class Database
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class DHTDiscovery
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class KnowledgeCommunity
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class LibTorrent
    {
        [JsonProperty("download_defaults", NullValueHandling = NullValueHandling.Ignore)]
        public LibTorrentDownloadDefaults DownloadDefaults { get; set; }

        // contains a lot more data, but it's not needed currently
    }

    public class Recommender
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class Rendezvous
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class TorrentChecker
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class TunnelCommunity
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("min_circuits", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinCircuits { get; set; }

        [JsonProperty("max_circuits", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxCircuits { get; set; }
    }

    public class Versioning
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    public class WatchFolder
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
        [JsonProperty("directory", NullValueHandling = NullValueHandling.Ignore)]
        public string Directory { get; set; }
        [JsonProperty("check_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int? CheckInterval { get; set; }
    }

    public class LibTorrentDownloadDefaults
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
