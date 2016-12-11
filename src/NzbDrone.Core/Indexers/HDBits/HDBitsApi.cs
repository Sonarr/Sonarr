using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class TorrentQuery
    {
        [JsonProperty(Required = Required.Always)]
        public string Username { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Passkey { get; set; }

        public string Hash { get; set; }

        public string Search { get; set; }

        public int[] Category { get; set; }

        public int[] Codec { get; set; }

        public int[] Medium { get; set; }

        public int[] Origin { get; set; }

        [JsonProperty(PropertyName = "imdb")]
        public ImdbInfo ImdbInfo { get; set; }

        [JsonProperty(PropertyName = "tvdb")]
        public TvdbInfo TvdbInfo { get; set; }

        [JsonProperty(PropertyName = "file_in_torrent")]
        public string FileInTorrent { get; set; }

        [JsonProperty(PropertyName = "snatched_only")]
        public bool? SnatchedOnly { get; set; }
        public int? Limit { get; set; }
        public int? Page { get; set; }

        public TorrentQuery Clone()
        {
            return MemberwiseClone() as TorrentQuery;
        }
    }

    public class HDBitsResponse
    {
        [JsonProperty(Required = Required.Always)]
        public StatusCode Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class TorrentQueryResponse
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public int Leechers { get; set; }
        public int Seeders { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "times_completed")]

        public uint TimesCompleted { get; set; }

        public long Size { get; set; }

        [JsonProperty(PropertyName = "utadded")]
        public long UtAdded { get; set; }

        public DateTime Added { get; set; }

        public uint Comments { get; set; }

        [JsonProperty(PropertyName = "numfiles")]
        public uint NumFiles { get; set; }

        [JsonProperty(PropertyName = "filename")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "freeleech")]
        public string FreeLeech { get; set; }

        [JsonProperty(PropertyName = "type_category")]
        public int TypeCategory { get; set; }

        [JsonProperty(PropertyName = "type_codec")]
        public int TypeCodec { get; set; }

        [JsonProperty(PropertyName = "type_medium")]
        public int TypeMedium { get; set; }

        [JsonProperty(PropertyName = "type_origin")]
        public int TypeOrigin { get; set; }

        [JsonProperty(PropertyName = "imdb")]
        public ImdbInfo ImdbInfo { get; set; }

        [JsonProperty(PropertyName = "tvdb")]
        public TvdbInfo TvdbInfo { get; set; }
    }

    public class ImdbInfo
    {
        public int? Id { get; set; }
        public string EnglishTitle { get; set; }
        public string OriginalTitle { get; set; }
        public int? Year { get; set; }
        public string[] Genres { get; set; }
        public float? Rating { get; set; }
    }

    public class TvdbInfo
    {
        public int? Id { get; set; }
        public int? Season { get; set; }
        public int? Episode { get; set; }
    }

    public enum StatusCode
    {
        Success = 0,
        Failure = 1,
        SslRequired = 2,
        JsonMalformed = 3,
        AuthDataMissing = 4,
        AuthFailed = 5,
        MissingRequiredParameters = 6,
        InvalidParameter = 7,
        ImdbImportFail = 8,
        ImdbTvNotAllowed = 9
    }
}
