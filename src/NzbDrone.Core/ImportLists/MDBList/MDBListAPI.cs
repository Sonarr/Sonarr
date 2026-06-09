using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListResponse
    {
        public List<MDBListItemResource> Shows { get; set; }
        public List<MDBListItemResource> Movies { get; set; }
    }

    public class MDBListItemResource
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public int Adult { get; set; }
        public string Title { get; set; }

        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }

        [JsonProperty("tvdb_id")]
        public int? TvdbId { get; set; }

        [JsonProperty("tvdbid")]
        public int? LegacyTvdbId { get; set; }

        public string Mediatype { get; set; }

        [JsonProperty("release_year")]
        public int? ReleaseYear { get; set; }

        public MDBListIdsResource Ids { get; set; }
    }

    public class MDBListIdsResource
    {
        public string Mdblist { get; set; }
        public string Imdb { get; set; }
        public int? Tmdb { get; set; }
        public int? Tvdb { get; set; }
    }
}
