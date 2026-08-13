using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public class TmdbFindResponse
    {
        [JsonProperty("tv_results")]
        public List<TmdbFindTvResult> TvResults { get; set; }
    }

    public class TmdbFindTvResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class TmdbExternalIdsResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tvdb_id")]
        public int? TvdbId { get; set; }
    }

    public class TmdbTranslationsResponse
    {
        [JsonProperty("translations")]
        public List<TmdbTranslation> Translations { get; set; }
    }

    public class TmdbTranslation
    {
        [JsonProperty("iso_639_1")]
        public string Iso639_1 { get; set; }

        [JsonProperty("data")]
        public TmdbTranslationData Data { get; set; }
    }

    public class TmdbTranslationData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }
    }

    public class TmdbSearchResponse
    {
        [JsonProperty("results")]
        public List<TmdbSearchResult> Results { get; set; }
    }

    public class TmdbSearchResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("first_air_date")]
        public string FirstAirDate { get; set; }
    }
}
