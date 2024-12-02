using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioTorrent
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }

        public long Downloaded { get; set; }
        public long Uploaded { get; set; }
        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }
        [JsonProperty(PropertyName = "estimated_time")]
        public long EstimatedTime { get; set; }
        [JsonProperty(PropertyName = "file_id")]
        public long FileId { get; set; }
        [JsonProperty(PropertyName = "percent_done")]
        public int PercentDone { get; set; }
        [JsonProperty(PropertyName = "seconds_seeding")]
        public long SecondsSeeding { get; set; }
        public long Size { get; set; }
        public string Status { get; set; }
        [JsonProperty(PropertyName = "save_parent_id")]
        public long SaveParentId { get; set; }
        [JsonProperty(PropertyName = "current_ratio")]
        public double Ratio { get; set; }
    }

    public class PutioTorrentMetadata
    {
        public static PutioTorrentMetadata fromTorrent(PutioTorrent torrent, bool downloaded = false)
        {
            return new PutioTorrentMetadata
            {
                Downloaded = downloaded,
                Id = torrent.Id
            };
        }

        public bool Downloaded { get; set; }

        public long Id { get; set; }
    }
}
