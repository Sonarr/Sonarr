using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListItemsResponse
    {
        public List<MDBListItemResource> Shows { get; set; }
    }

    public class MDBListItemResource
    {
        public string Title { get; set; }

        [JsonProperty("release_year")]
        public int? ReleaseYear { get; set; }

        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }

        [JsonProperty("tvdb_id")]
        public int? TvdbId { get; set; }

        public MDBListIdsResource Ids { get; set; }
    }

    public class MDBListIdsResource
    {
        public string Imdb { get; set; }
        public int? Tmdb { get; set; }
        public int? Tvdb { get; set; }
    }

    public class MDBListTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        public string Error { get; set; }
    }

    public class MDBListErrorResponse
    {
        public string Error { get; set; }
    }

    public class MDBListUserResponse
    {
        public string Username { get; set; }
    }
}
