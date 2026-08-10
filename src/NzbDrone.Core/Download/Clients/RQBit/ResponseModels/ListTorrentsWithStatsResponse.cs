using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels
{
    public class ListTorrentsWithStatsResponse
    {
        public List<TorrentWithStatsResponse> Torrents { get; set; }
    }

    public class TorrentWithStatsResponse
    {
        public long Id { get; set; }

        [JsonProperty("info_hash")]
        public string InfoHash { get; set; }

        public string Name { get; set; }

        [JsonProperty("output_folder")]
        public string OutputFolder { get; set; }

        public TorrentStatsResponse Stats { get; set; }
    }

    public class TorrentStatsResponse
    {
        public TorrentState State { get; set; }

        [JsonProperty("file_progress")]
        public List<long> FileProgress { get; set; }

        public string Error { get; set; }

        [JsonProperty("progress_bytes")]
        public long ProgressBytes { get; set; }

        [JsonProperty("uploaded_bytes")]
        public long UploadedBytes { get; set; }

        [JsonProperty("total_bytes")]
        public long TotalBytes { get; set; }

        public bool Finished { get; set; }
        public TorrentLiveStatsResponse Live { get; set; }
    }

    public class TorrentLiveStatsResponse
    {
        public TorrentSnapshotResponse Snapshot { get; set; }

        [JsonProperty("download_speed")]
        public TorrentSpeedResponse DownloadSpeed { get; set; }

        [JsonProperty("upload_speed")]
        public TorrentSpeedResponse UploadSpeed { get; set; }

        [JsonProperty("time_remaining")]
        public TorrentTimeRemainingResponse TimeRemaining { get; set; }
    }

    public class TorrentSnapshotResponse
    {
        [JsonProperty("downloaded_and_checked_bytes")]
        public long DownloadedAndCheckedBytes { get; set; }

        [JsonProperty("fetched_bytes")]
        public long FetchedBytes { get; set; }

        [JsonProperty("uploaded_bytes")]
        public long UploadedBytes { get; set; }

        [JsonProperty("peer_stats")]
        public TorrentPeerStatsResponse PeerStats { get; set; }
    }

    public class TorrentSpeedResponse
    {
        public double Mbps { get; set; }

        [JsonProperty("human_readable")]
        public string HumanReadable { get; set; }
    }

    public class TorrentTimeRemainingResponse
    {
        public TorrentDurationResponse Duration { get; set; }

        [JsonProperty("human_readable")]
        public string HumanReadable { get; set; }
    }

    public class TorrentDurationResponse
    {
        public long Secs { get; set; }
        public long Nanos { get; set; }
    }

    public class TorrentPeerStatsResponse
    {
        public int Queued { get; set; }
        public int Connecting { get; set; }
        public int Live { get; set; }
        public int Seen { get; set; }
        public int Dead { get; set; }

        [JsonProperty("not_needed")]
        public int NotNeeded { get; set; }

        public int Steals { get; set; }
    }
}
