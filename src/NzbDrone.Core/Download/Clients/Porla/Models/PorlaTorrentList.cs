using System.Collections.ObjectModel;
using Newtonsoft.Json;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;

namespace NzbDrone.Core.Download.Clients.Porla.Models
{
    /// <summary> Implementation of the <em>Torrents</em> field type in the response data from <a href="https://github.com/porla/porla/blob/v0.37.0/src/methods/torrentslist_reqres.hpp">torrentslist_reqres.hpp</a></summary>
    public sealed class PorlaTorrentDetail
    {
        /// <summary> cumulative counter in the active state means not paused and added to session </summary>
        [JsonProperty("active_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long ActiveDuration { get; set; }

        /// <summary> are accumulated download payload byte counters. They are saved in and restored from resume data to keep totals across sessions. </summary>
        [JsonProperty("all_time_download", NullValueHandling = NullValueHandling.Ignore)]
        public long AllTimeDownload { get; set; }

        /// <summary> are accumulated upload payload byte counters. They are saved in and restored from resume data to keep totals across sessions. </summary>
        [JsonProperty("all_time_upload", NullValueHandling = NullValueHandling.Ignore)]
        public long AllTimeUpload { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        /// <summary> the total rates for all peers for this torrent. These will usually have better precision than summing the rates from all peers. The rates are given as the number of bytes per second. </summary>
        [JsonProperty("download_rate", NullValueHandling = NullValueHandling.Ignore)]
        public int DownloadRate { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        /// <summary> Estimated Time of Arrivial. The estimated amount of seconds until the torrent finishes downloading. -1 indicates forever </summary>
        [JsonProperty("eta", NullValueHandling = NullValueHandling.Ignore)]
        public long ETA { get; set; }

        /// <summary> cumulative counter in the fisished means all selected files/pieces were downloaded and available to other peers (this is always a subset of active time) </summary>
        [JsonProperty("finished_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long FinishedDuration { get; set; }

        /// <summary> reflects several of the torrent's flags </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<string> Flags { get; set; }

        [JsonProperty("info_hash", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(LibTorrentInfoHashConverter))]
        public LibTorrentInfoHash InfoHash { get; set; }

        /// <summary> the timestamps of the last time this downloaded payload from any peer. (might be relative) </summary>
        [JsonProperty("last_download", NullValueHandling = NullValueHandling.Ignore)]
        public long LastDownload { get; set; }

        /// <summary> the timestamps of the last time this uploaded payload to any peer. (might be relative) </summary>
        [JsonProperty("last_upload", NullValueHandling = NullValueHandling.Ignore)]
        public long LastUpload { get; set; }

        /// <summary> the total number of peers (including seeds). We are not necessarily connected to all the peers in our peer list. This is the number of peers we know of in total, including banned peers and peers that we have failed to connect to. </summary>
        [JsonProperty("list_peers", NullValueHandling = NullValueHandling.Ignore)]
        public int ListPeers { get; set; }

        /// <summary> the number of seeds in our peer list </summary>
        [JsonProperty("list_seeds", NullValueHandling = NullValueHandling.Ignore)]
        public int ListSeeds { get; set; }

        // technically any valid json should be able to fit here. including `[]`
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyDictionary<string, string> Metadata { get; set; }

        /// <summary> this is true if this torrent's storage is currently being moved from one location to another. This may potentially be a long operation if a large file ends up being copied from one drive to another. </summary>
        [JsonProperty("moving_storage", NullValueHandling = NullValueHandling.Ignore)]
        public bool MovingStorage { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary> the number of peers this torrent currently is connected to. Peer connections that are in the half-open state (is attempting to connect) or are queued for later connection attempt do not count. </summary>
        [JsonProperty("num_peers", NullValueHandling = NullValueHandling.Ignore)]
        public int NumPeers { get; set; }

        /// <summary> the number of peers that are seeding that this client is currently connected to. </summary>
        [JsonProperty("num_seeds", NullValueHandling = NullValueHandling.Ignore)]
        public int NumSeeds { get; set; }

        /// <summary> ratio of upload / downloaded </summary>
        [JsonProperty("progress", NullValueHandling = NullValueHandling.Ignore)]
        public float Progress { get; set; }

        /// <summary> the position this torrent has in the download queue. If the torrent is a seed or finished, this is -1. </summary>
        [JsonProperty("queue_position", NullValueHandling = NullValueHandling.Ignore)]
        public int QueuePosition { get; set; }

        /// <summary> ratio of upload / downloaded </summary>
        [JsonProperty("ratio", NullValueHandling = NullValueHandling.Ignore)]
        public double Ratio { get; set; }

        /// <summary> the path to which the torrent is downloaded to </summary>
        [JsonProperty("save_path", NullValueHandling = NullValueHandling.Ignore)]
        public string SavePath { get; set; }

        /// <summary> cumulative counter in the seeding means all files/pieces were downloaded and available to peers </summary>
        [JsonProperty("seeding_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long SeedingDuration { get; set; }

        /// <summary> name of the session this torrent is a part of </summary>
        [JsonProperty("session", NullValueHandling = NullValueHandling.Ignore)]
        public string Session { get; set; }

        /// <summary> the total number of bytes the torrent-file represents. Note that this is the number of pieces times the piece size (modulo the last piece possibly being smaller). With pad files, the total size will be larger than the sum of all (regular) file sizes. </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long Size { get; set; }

        /// <summary> the main state the torrent is in </summary>
        /// <see cref="LibTorrentStatus"/>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public LibTorrentStatus State { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<string> Tags { get; set; }

        /// <summary> the total number of bytes to download for this torrent. This may be less than the size of the torrent in case there are pad files. This number only counts bytes that will actually be requested from peers. </summary>
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long Total { get; set; }

        /// <summary> the total number of bytes of the file(s) that we have. All this does not necessarily has to be downloaded during this session </summary>
        [JsonProperty("total_done", NullValueHandling = NullValueHandling.Ignore)]
        public long TotalDone { get; set; }

        /// <summary> the total rates for all peers for this torrent. These will usually have better precision than summing the rates from all peers. The rates are given as the number of bytes per second. </summary>
        [JsonProperty("upload_rate", NullValueHandling = NullValueHandling.Ignore)]
        public int UploadRate { get; set; }
    }

    /// <summary> Implementation of the torrent response data type from <a href="https://github.com/porla/porla/blob/v0.37.0/src/methods/torrentslist_reqres.hpp">torrentslist_reqres.hpp</a></summary>
    public sealed class ResponsePorlaTorrentList
    {
        [JsonProperty("order_by", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderBy { get; set; }

        [JsonProperty("order_by_dir", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderByDir { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int Page { get; set; }

        [JsonProperty("page_size", NullValueHandling = NullValueHandling.Ignore)]
        public int PageSize { get; set; }

        [JsonProperty("torrents", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PorlaTorrentDetail> Torrents { get; set; }

        [JsonProperty("torrents_total", NullValueHandling = NullValueHandling.Ignore)]
        public int TorrentsTotal { get; set; }

        [JsonProperty("torrents_total_unfiltered", NullValueHandling = NullValueHandling.Ignore)]
        public int TorrentsTotalUnfiltered { get; set; }
    }
}
