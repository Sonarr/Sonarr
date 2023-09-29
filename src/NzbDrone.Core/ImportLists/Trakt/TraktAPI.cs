using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("aired_episodes")]
        public int AiredEpisodes { get; set; }
    }

    public class TraktResponse
    {
        public TraktSeriesResource Show { get; set; }
    }

    public class TraktWatchedEpisodeResource
    {
        public int? Plays { get; set; }
    }

    public class TraktWatchedSeasonResource
    {
        public int? Number { get; set; }
        public List<TraktWatchedEpisodeResource> Episodes { get; set; }
    }

    public class TraktWatchedResponse : TraktResponse
    {
        public List<TraktWatchedSeasonResource> Seasons { get; set; }
    }

    public class RefreshRequestResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
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
