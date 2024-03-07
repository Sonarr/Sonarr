using System.Collections.ObjectModel;
using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    public sealed class PorlaTorrentDetail
    {
        [JsonProperty("active_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long ActiveDuration { get; set; }

        [JsonProperty("all_time_download", NullValueHandling = NullValueHandling.Ignore)]
        public long AllTimeDownload { get; set; }

        [JsonProperty("all_time_upload", NullValueHandling = NullValueHandling.Ignore)]
        public long AllTimeUpload { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("download_rate", NullValueHandling = NullValueHandling.Ignore)]
        public long DownloadRate { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("eta", NullValueHandling = NullValueHandling.Ignore)]
        public long ETA { get; set; }

        [JsonProperty("finished_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long FinishedDuration { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<string> Flags { get; set; }

        [JsonProperty("info_hash", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(LibTorrentInfoHashConverter))]
        public LibTorrentInfoHash InfoHash { get; set; }

        [JsonProperty("last_download", NullValueHandling = NullValueHandling.Ignore)]
        public long LastDownload { get; set; }

        [JsonProperty("last_upload", NullValueHandling = NullValueHandling.Ignore)]
        public long LastUpload { get; set; }

        [JsonProperty("list_peers", NullValueHandling = NullValueHandling.Ignore)]
        public long ListPeers { get; set; }

        [JsonProperty("list_seeds", NullValueHandling = NullValueHandling.Ignore)]
        public long ListSeeds { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyDictionary<string, string> Metadata { get; set; }

        [JsonProperty("moving_storage", NullValueHandling = NullValueHandling.Ignore)]
        public bool MovingStorage { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("num_peers", NullValueHandling = NullValueHandling.Ignore)]
        public long NumPeers { get; set; }

        [JsonProperty("num_seeds", NullValueHandling = NullValueHandling.Ignore)]
        public long NumSeeds { get; set; }

        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public double Progress { get; set; }

        [JsonProperty("queue_position", NullValueHandling = NullValueHandling.Ignore)]
        public long QueuePosition { get; set; }

        [JsonProperty("ratio", NullValueHandling = NullValueHandling.Ignore)]
        public double Ratio { get; set; }

        [JsonProperty("save_path", NullValueHandling = NullValueHandling.Ignore)]
        public string SavePath { get; set; }

        [JsonProperty("seeding_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long SeedingDuration { get; set; }

        [JsonProperty("session", NullValueHandling = NullValueHandling.Ignore)]
        public string Session { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long Size { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public LibTorrentStatus State { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<string> Tags { get; set; }

        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long Total { get; set; }

        [JsonProperty("total_done", NullValueHandling = NullValueHandling.Ignore)]
        public long TotalDone { get; set; }

        [JsonProperty("upload_rate", NullValueHandling = NullValueHandling.Ignore)]
        public long UploadRate { get; set; }
    }

    public sealed class ResponsePorlaTorrentList
    {
        [JsonProperty("order_by", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderBy { get; set; }

        [JsonProperty("order_by_dir", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderByDir { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public long Page { get; set; }

        [JsonProperty("page_size", NullValueHandling = NullValueHandling.Ignore)]
        public long PageSize { get; set; }

        [JsonProperty("torrents", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaTorrentDetail> Torrents { get; set; }

        [JsonProperty("torrents_total", NullValueHandling = NullValueHandling.Ignore)]
        public long TorrentsTotal { get; set; }

        [JsonProperty("torrents_total_unfiltered", NullValueHandling = NullValueHandling.Ignore)]
        public long TorrentsTotalUnfiltered { get; set; }
    }
}
