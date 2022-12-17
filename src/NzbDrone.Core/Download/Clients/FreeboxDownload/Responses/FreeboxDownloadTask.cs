using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.FreeboxDownload.Responses
{
    public enum FreeboxDownloadTaskType
    {
        Bt,
        Nzb,
        Http,
        Ftp
    }

    public enum FreeboxDownloadTaskStatus
    {
        Unknown,
        Stopped,
        Queued,
        Starting,
        Downloading,
        Stopping,
        Error,
        Done,
        Checking,
        Repairing,
        Extracting,
        Seeding,
        Retry
    }

    public enum FreeboxDownloadTaskIoPriority
    {
        Low,
        Normal,
        High
    }

    public class FreeboxDownloadTask
    {
        private static readonly Dictionary<string, string> Descriptions;

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "download_dir")]
        public string DownloadDirectory { get; set; }
        public string DecodedDownloadDirectory
        {
            get
            {
                return DownloadDirectory.DecodeBase64();
            }
            set
            {
                DownloadDirectory = value.EncodeBase64();
            }
        }

        [JsonProperty(PropertyName = "info_hash")]
        public string InfoHash { get; set; }
        [JsonProperty(PropertyName = "queue_pos")]
        public int QueuePosition { get; set; }
        [JsonConverter(typeof(UnderscoreStringEnumConverter), FreeboxDownloadTaskStatus.Unknown)]
        public FreeboxDownloadTaskStatus Status { get; set; }
        [JsonProperty(PropertyName = "eta")]
        public long Eta { get; set; }
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "io_priority")]
        public string IoPriority { get; set; }
        [JsonProperty(PropertyName = "stop_ratio")]
        public long StopRatio { get; set; }
        [JsonProperty(PropertyName = "piece_length")]
        public long PieceLength { get; set; }
        [JsonProperty(PropertyName = "created_ts")]
        public long CreatedTimestamp { get; set; }
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }
        [JsonProperty(PropertyName = "rx_pct")]
        public long ReceivedPrct { get; set; }
        [JsonProperty(PropertyName = "rx_bytes")]
        public long ReceivedBytes { get; set; }
        [JsonProperty(PropertyName = "rx_rate")]
        public long ReceivedRate { get; set; }
        [JsonProperty(PropertyName = "tx_pct")]
        public long TransmittedPrct { get; set; }
        [JsonProperty(PropertyName = "tx_bytes")]
        public long TransmittedBytes { get; set; }
        [JsonProperty(PropertyName = "tx_rate")]
        public long TransmittedRate { get; set; }

        static FreeboxDownloadTask()
        {
            Descriptions = new Dictionary<string, string>
            {
                { "internal", "Internal error." },
                { "disk_full", "The disk is full." },
                { "unknown", "Unknown error." },
                { "parse_error", "Parse error." },
                { "unknown_host", "Unknown host." },
                { "timeout", "Timeout." },
                { "bad_authentication", "Invalid credentials." },
                { "connection_refused", "Remote host refused connection." },
                { "bt_tracker_error", "Unable to announce on tracker." },
                { "bt_missing_files", "Missing torrent files." },
                { "bt_file_error", "Error accessing torrent files." },
                { "missing_ctx_file", "Error accessing task context file." },
                { "nzb_no_group", "Cannot find the requested group on server." },
                { "nzb_not_found", "Article not fount on the server." },
                { "nzb_invalid_crc", "Invalid article CRC." },
                { "nzb_invalid_size", "Invalid article size." },
                { "nzb_invalid_filename", "Invalid filename." },
                { "nzb_open_failed", "Error opening." },
                { "nzb_write_failed", "Error writing." },
                { "nzb_missing_size", "Missing article size." },
                { "nzb_decode_error", "Article decoding error." },
                { "nzb_missing_segments", "Missing article segments." },
                { "nzb_error", "Other nzb error." },
                { "nzb_authentication_required", "Nzb server need authentication." }
            };
        }

        public string GetErrorDescription()
        {
            if (Descriptions.ContainsKey(Error))
            {
                return Descriptions[Error];
            }

            return $"{Error} - Unknown error";
        }
    }
}
