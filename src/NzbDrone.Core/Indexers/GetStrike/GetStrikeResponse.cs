using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers.GetStrike
{
    public class GetStrikeResponse
    {
        [JsonProperty(Required = Required.Always)]
        public int statuscode { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int results { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double responsetime { get; set; }
        public object torrents { get; set; }
    }

    public class GetStrikeNotFound
    {
        [JsonProperty(Required = Required.Always)]
        public int statuscode { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string message { get; set; }
    }

    public class GetStrikeQueryResponse
    {
        public string torrent_hash { get; set; }
        public string torrent_title { get; set; }
        public string torrent_category { get; set; }
        public string sub_category { get; set; }
        public int seeds { get; set; }
        public int leeches { get; set; }
        public uint file_count { get; set; }
        public long Size { get; set; }
        public string upload_date { get; set; }
        public string uploader_username { get; set; }
        public uint download_count { get; set; }
        public string magnet_uri { get; set; }
        public string page { get; set; }
    }
 }
