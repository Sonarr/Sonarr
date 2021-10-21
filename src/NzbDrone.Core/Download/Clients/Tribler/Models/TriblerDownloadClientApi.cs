namespace NzbDrone.Core.Download.Clients.Tribler
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum DownloadStatus
    {
        [System.Runtime.Serialization.EnumMember(Value = @"WAITING4HASHCHECK")]
        WAITING4HASHCHECK = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"HASHCHECKING")]
        HASHCHECKING = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"METADATA")]
        METADATA = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"DOWNLOADING")]
        DOWNLOADING = 3,

        [System.Runtime.Serialization.EnumMember(Value = @"SEEDING")]
        SEEDING = 4,

        [System.Runtime.Serialization.EnumMember(Value = @"STOPPED")]
        STOPPED = 5,

        [System.Runtime.Serialization.EnumMember(Value = @"ALLOCATING_DISKSPACE")]
        ALLOCATING_DISKSPACE = 6,

        [System.Runtime.Serialization.EnumMember(Value = @"EXIT_NODES")]
        EXIT_NODES = 7,

        [System.Runtime.Serialization.EnumMember(Value = @"CIRCUITS")]
        CIRCUITS = 8,

        [System.Runtime.Serialization.EnumMember(Value = @"STOPPED_ON_ERROR")]
        STOPPED_ON_ERROR = 9,
        
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Trackers
    {
        /// <summary>url of tracker</summary>
        [Newtonsoft.Json.JsonProperty("url", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>number of peers</summary>
        [Newtonsoft.Json.JsonProperty("peers", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public object Peers { get; set; }

        /// <summary>If the tracker is working or not.</summary>
        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Status { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Download
    {
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("progress", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Progress { get; set; }

        [Newtonsoft.Json.JsonProperty("anon_download", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Anon_download { get; set; }

        [Newtonsoft.Json.JsonProperty("availability", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Availability { get; set; }

        [Newtonsoft.Json.JsonProperty("eta", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double? Eta { get; set; }

        [Newtonsoft.Json.JsonProperty("total_pieces", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Total_pieces { get; set; }

        [Newtonsoft.Json.JsonProperty("num_seeds", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Num_seeds { get; set; }

        [Newtonsoft.Json.JsonProperty("total_up", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Total_up { get; set; }

        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public DownloadStatus? Status { get; set; }

        [Newtonsoft.Json.JsonProperty("infohash", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Infohash { get; set; }

        [Newtonsoft.Json.JsonProperty("ratio", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Ratio { get; set; }

        [Newtonsoft.Json.JsonProperty("vod_mode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Vod_mode { get; set; }

        [Newtonsoft.Json.JsonProperty("time_added", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Time_added { get; set; }

        [Newtonsoft.Json.JsonProperty("max_upload_speed", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Max_upload_speed { get; set; }

        [Newtonsoft.Json.JsonProperty("max_download_speed", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Max_download_speed { get; set; }

        [Newtonsoft.Json.JsonProperty("hops", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Hops { get; set; }

        [Newtonsoft.Json.JsonProperty("safe_seeding", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Safe_seeding { get; set; }

        [Newtonsoft.Json.JsonProperty("error", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Error { get; set; }

        [Newtonsoft.Json.JsonProperty("total_down", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Total_down { get; set; }

        [Newtonsoft.Json.JsonProperty("vod_prebuffering_progress", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Vod_prebuffering_progress { get; set; }

        [Newtonsoft.Json.JsonProperty("trackers", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Trackers> Trackers { get; set; }

        [Newtonsoft.Json.JsonProperty("size", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [Newtonsoft.Json.JsonProperty("peers", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> Peers { get; set; }

        [Newtonsoft.Json.JsonProperty("destination", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Destination { get; set; }

        [Newtonsoft.Json.JsonProperty("speed_down", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Speed_down { get; set; }

        [Newtonsoft.Json.JsonProperty("speed_up", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Speed_up { get; set; }

        [Newtonsoft.Json.JsonProperty("vod_prebuffering_progress_consec", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Vod_prebuffering_progress_consec { get; set; }

        [Newtonsoft.Json.JsonProperty("files", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> Files { get; set; }

        [Newtonsoft.Json.JsonProperty("num_peers", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Num_peers { get; set; }

        [Newtonsoft.Json.JsonProperty("channel_download", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? ChannelDownload { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class DownloadsResponse
    {
        [Newtonsoft.Json.JsonProperty("downloads", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Download> Downloads { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class AddDownloadRequest
    {
        /// <summary>Number of hops for the anonymous download. No hops is equivalent to a plain download</summary>
        [Newtonsoft.Json.JsonProperty("anon_hops", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Anon_hops { get; set; }

        /// <summary>Whether the seeding of the download should be anonymous or not</summary>
        [Newtonsoft.Json.JsonProperty("safe_seeding", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Safe_seeding { get; set; }

        /// <summary>the download destination path of the torrent</summary>
        [Newtonsoft.Json.JsonProperty("destination", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Destination { get; set; }

        /// <summary>The URI of the torrent file that should be downloaded. This URI can either represent a file location, a magnet link or a HTTP(S) url.</summary>
        [Newtonsoft.Json.JsonProperty("uri", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Uri { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class AddDownloadResponse
    {
        [Newtonsoft.Json.JsonProperty("infohash", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Infohash { get; set; }

        [Newtonsoft.Json.JsonProperty("started", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Started { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class RemoveDownloadRequest
    {
        /// <summary>Whether or not to remove the associated data</summary>
        [Newtonsoft.Json.JsonProperty("remove_data", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Remove_data { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class DeleteDownloadResponse
    {
        [Newtonsoft.Json.JsonProperty("removed", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Removed { get; set; }

        [Newtonsoft.Json.JsonProperty("infohash", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Infohash { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class UpdateDownloadRequest
    {
        /// <summary>The anonymity of a download can be changed at runtime by passing the anon_hops parameter, however, this must be the only parameter in this request.</summary>
        [Newtonsoft.Json.JsonProperty("anon_hops", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Anon_hops { get; set; }

        [Newtonsoft.Json.JsonProperty("selected_files", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<int> Selected_files { get; set; }

        /// <summary>State parameter to be passed to modify the state of the download (resume/stop/recheck)</summary>
        [Newtonsoft.Json.JsonProperty("state", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string State { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class UpdateDownloadResponse
    {
        [Newtonsoft.Json.JsonProperty("modified", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Modified { get; set; }

        [Newtonsoft.Json.JsonProperty("infohash", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Infohash { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class File
    {
        [Newtonsoft.Json.JsonProperty("size", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [Newtonsoft.Json.JsonProperty("index", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? Index { get; set; }

        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("progress", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public float? Progress { get; set; }

        [Newtonsoft.Json.JsonProperty("included", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Included { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.5.2.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class GetFilesResponse
    {
        [Newtonsoft.Json.JsonProperty("files", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<File> Files { get; set; }
    }
}
