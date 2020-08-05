using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktSeriesIdsResource
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int? Tmdb { get; set; }
        public int? Tvdb { get; set; }
    }

    public class TraktSeriesResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktSeriesIdsResource Ids { get; set; }
    }

    public class TraktResponse
    {
        public TraktSeriesResource Show { get; set; }
    }

    public class RefreshRequestResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class UserSettingsResponse
    {
        public TraktUserResource User { get; set; }
    }

    public class TraktUserResource
    {
        public string Username { get; set; }
        public TraktUserIdsResource Ids { get; set; }
    }

    public class TraktUserIdsResource
    {
        public string Slug { get; set; }
    }
}
