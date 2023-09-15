using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioTorrent
    {
        public long Downloaded { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "estimated_time")]
        public long EstimatedTime { get; set; }

        [JsonProperty(PropertyName = "file_id")]
        public long FileId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(PropertyName = "percent_done")]
        public int PercentDone { get; set; }

        [JsonProperty(PropertyName = "seconds_seeding")]
        public long SecondsSeeding { get; set; }

        public long Size { get; set; }

        public string Status { get; set; }
    }
}
