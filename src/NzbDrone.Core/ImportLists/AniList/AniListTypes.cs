using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.AniList
{
    /// <summary>
    /// Media list status types
    /// </summary>
    public static class MediaListStatus
    {
        /// <summary>
        /// Currently Watching Anime Series
        /// </summary>
        public const string Current = "CURRENT";

        /// <summary>
        /// Plan to Watch Anime Series
        /// </summary>
        public const string Planning = "PLANNING";

        /// <summary>
        /// Completed Anime Series
        /// </summary>
        public const string Completed = "COMPLETED";

        /// <summary>
        /// Dropped Anime Series
        /// </summary>
        public const string Dropped = "DROPPED";

        /// <summary>
        /// On Hold Anime Series
        /// </summary>
        public const string Paused = "PAUSED";

        /// <summary>
        /// Rewatching Anime Series
        /// </summary>
        public const string Repeating = "REPEATING";
    }

    public static class MediaStatus
    {
        /// <summary>
        /// Anime series has finished airing
        /// </summary>
        public const string Finished = "FINISHED";

        /// <summary>
        /// Anime series is currently airing
        /// </summary>
        public const string Releasing = "RELEASING";

        /// <summary>
        /// Anime series had not yet begun airing
        /// </summary>
        public const string Unreleased = "NOT_YET_RELEASED";

        /// <summary>
        /// Anime series was cancelled
        /// </summary>
        public const string Cancelled = "CANCELLED";

        /// <summary>
        /// Anime series is currently on hiatus
        /// </summary>
        public const string Hiatus = "HIATUS";
    }

    /// <summary>
    /// Data during token refresh cycles
    /// </summary>
    public class RefreshRequestResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Mapping data between anime service providers
    /// </summary>
    public class MediaMapping
    {
        /// <summary>
        /// Anilist ID
        /// </summary>
        [JsonPropertyName("anilist_id")]
        public int? Anilist { get; set; }

        /// <summary>
        /// The TVDB ID
        /// </summary>
        [JsonPropertyName("thetvdb_id")]
        public int? TVDB { get; set; }

        /// <summary>
        /// My Anime List ID
        /// </summary>
        [JsonPropertyName("mal_id")]
        public int? MyAnimeList { get; set; }

        /// <summary>
        /// IMDB ID
        /// </summary>
        [JsonPropertyName("imdb_id")]
        public string IMDB { get; set; }

        /// <summary>
        /// Entry type such as TV or MOVIE.
        ///
        /// Required when mapping between services that reuse ids for different content types.
        /// </summary>
        [JsonPropertyName("type")]
        public string ItemType { get; set; }
    }
}
