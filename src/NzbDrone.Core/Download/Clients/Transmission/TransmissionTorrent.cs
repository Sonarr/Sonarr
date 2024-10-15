using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionTorrent
    {
        public int Id { get; set; }
        public string HashString { get; set; }
        public string Name { get; set; }
        public string DownloadDir { get; set; }
        public long TotalSize { get; set; }
        public long LeftUntilDone { get; set; }
        public bool IsFinished { get; set; }
        public IReadOnlyCollection<string> Labels { get; set; } = Array.Empty<string>();
        public long Eta { get; set; }
        public TransmissionTorrentStatus Status { get; set; }
        public long SecondsDownloading { get; set; }
        public long SecondsSeeding { get; set; }
        public string ErrorString { get; set; }
        public long DownloadedEver { get; set; }
        public long UploadedEver { get; set; }
        public double SeedRatioLimit { get; set; }
        public int SeedRatioMode { get; set; }
        public long SeedIdleLimit { get; set; }
        public int SeedIdleMode { get; set; }
        public int FileCount => TransmissionFileCount ?? VuzeFileCount ?? 0;

        [JsonProperty(PropertyName = "file-count")]
        public int? TransmissionFileCount { get; set; }

        [JsonProperty(PropertyName = "fileCount")]
        public int? VuzeFileCount { get; set; }
    }
}
