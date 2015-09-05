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
        [JsonProperty(PropertyName = "torrent_hash")]
        public string Hash { get; set; }
        [JsonProperty(PropertyName = "torrent_title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "torrent_category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "sub_category")]
        public string Subcategory { get; set; }

        [JsonProperty(PropertyName = "seeds")]
        public int Seeders { get; set; }
        [JsonProperty(PropertyName = "leeches")]
        public int Leechers { get; set; }

        [JsonProperty(PropertyName = "file_count")]
        public uint NumFiles { get; set; }

        public long Size { get; set; }

        [JsonProperty(PropertyName = "upload_date")]
        public string Added { get; set; }

        [JsonProperty(PropertyName = "uploader_username")]
        public string Uploader { get; set; }

        [JsonProperty(PropertyName = "download_count")]
        public uint TimesCompleted { get; set; }

        [JsonProperty(PropertyName = "magnet_uri")]
        public string MagnetUri { get; set; }

        [JsonProperty(PropertyName = "page")]
        public string Info { get; set; }

    }
 }
