using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public enum DownloadStatus
    {
        [EnumMember(Value = @"WAITING4HASHCHECK")]
        Waiting4HashCheck = 0,

        [EnumMember(Value = @"HASHCHECKING")]
        Hashchecking = 1,

        [EnumMember(Value = @"METADATA")]
        Metadata = 2,

        [EnumMember(Value = @"DOWNLOADING")]
        Downloading = 3,

        [EnumMember(Value = @"SEEDING")]
        Seeding = 4,

        [EnumMember(Value = @"STOPPED")]
        Stopped = 5,

        [EnumMember(Value = @"ALLOCATING_DISKSPACE")]
        AllocatingDiskspace = 6,

        [EnumMember(Value = @"EXIT_NODES")]
        Exitnodes = 7,

        [EnumMember(Value = @"CIRCUITS")]
        Circuits = 8,

        [EnumMember(Value = @"STOPPED_ON_ERROR")]
        StoppedOnError = 9,
    }

    public class Trackers
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("peers", NullValueHandling = NullValueHandling.Ignore)]
        public object Peers { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }

    public class Download
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public float? Progress { get; set; }

        [JsonProperty("anon_download", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AnonDownload { get; set; }

        [JsonProperty("availability", NullValueHandling = NullValueHandling.Ignore)]
        public float? Availability { get; set; }

        [JsonProperty("eta", NullValueHandling = NullValueHandling.Ignore)]
        public double? Eta { get; set; }

        [JsonProperty("total_pieces", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalPieces { get; set; }

        [JsonProperty("num_seeds", NullValueHandling = NullValueHandling.Ignore)]
        public long? NumSeeds { get; set; }

        [JsonProperty("total_up", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalUp { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DownloadStatus? Status { get; set; }

        [JsonProperty("infohash", NullValueHandling = NullValueHandling.Ignore)]
        public string Infohash { get; set; }

        [JsonProperty("ratio", NullValueHandling = NullValueHandling.Ignore)]
        public float? Ratio { get; set; }

        [JsonProperty("vod_mode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VideoOnDemandMode { get; set; }

        [JsonProperty("time_added", NullValueHandling = NullValueHandling.Ignore)]
        public long? TimeAdded { get; set; }

        [JsonProperty("max_upload_speed", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxUploadSpeed { get; set; }

        [JsonProperty("max_download_speed", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxDownloadSpeed { get; set; }

        [JsonProperty("hops", NullValueHandling = NullValueHandling.Ignore)]
        public long? Hops { get; set; }

        [JsonProperty("safe_seeding", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SafeSeeding { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("total_down", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalDown { get; set; }

        [JsonProperty("vod_prebuffering_progress", NullValueHandling = NullValueHandling.Ignore)]
        public float? VideoOnDemandPrebufferingProgress { get; set; }

        [JsonProperty("trackers", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Trackers> Trackers { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("peers", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Peers { get; set; }

        [JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
        public string Destination { get; set; }

        [JsonProperty("speed_down", NullValueHandling = NullValueHandling.Ignore)]
        public float? SpeedDown { get; set; }

        [JsonProperty("speed_up", NullValueHandling = NullValueHandling.Ignore)]
        public float? SpeedUp { get; set; }

        [JsonProperty("vod_prebuffering_progress_consec", NullValueHandling = NullValueHandling.Ignore)]
        public float? VideoOnDemandPrebufferingProgressConsec { get; set; }

        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Files { get; set; }

        [JsonProperty("num_peers", NullValueHandling = NullValueHandling.Ignore)]
        public long? NumPeers { get; set; }

        [JsonProperty("channel_download", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ChannelDownload { get; set; }
    }

    public class DownloadsResponse
    {
        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Download> Downloads { get; set; }
    }

    public class AddDownloadRequest
    {
        [JsonProperty("anon_hops", NullValueHandling = NullValueHandling.Ignore)]
        public long? AnonymityHops { get; set; }

        [JsonProperty("safe_seeding", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SafeSeeding { get; set; }

        [JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
        public string Destination { get; set; }

        [JsonProperty("uri", Required = Newtonsoft.Json.Required.Always)]
        [Required(AllowEmptyStrings = true)]
        public string Uri { get; set; }
    }

    public class AddDownloadResponse
    {
        [JsonProperty("infohash", NullValueHandling = NullValueHandling.Ignore)]
        public string Infohash { get; set; }

        [JsonProperty("started", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Started { get; set; }
    }

    public class RemoveDownloadRequest
    {
        [JsonProperty("remove_data", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RemoveData { get; set; }
    }

    public class DeleteDownloadResponse
    {
        [JsonProperty("removed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Removed { get; set; }

        [JsonProperty("infohash", NullValueHandling = NullValueHandling.Ignore)]
        public string Infohash { get; set; }
    }

    public class UpdateDownloadRequest
    {
        [JsonProperty("anon_hops", NullValueHandling = NullValueHandling.Ignore)]
        public long? AnonHops { get; set; }

        [JsonProperty("selected_files", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<int> Selected_files { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }
    }

    public class UpdateDownloadResponse
    {
        [JsonProperty("modified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Modified { get; set; }

        [JsonProperty("infohash", NullValueHandling = NullValueHandling.Ignore)]
        public string Infohash { get; set; }
    }

    public class File
    {
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public float? Progress { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Included { get; set; }
    }

    public class GetFilesResponse
    {
        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<File> Files { get; set; }
    }
}
