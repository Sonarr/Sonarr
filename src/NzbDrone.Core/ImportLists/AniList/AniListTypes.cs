using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.AniList
{
    public static class MediaListStatus
    {
        public const string Current = "CURRENT";

        public const string Planning = "PLANNING";

        public const string Completed = "COMPLETED";

        public const string Dropped = "DROPPED";

        public const string Paused = "PAUSED";

        public const string Repeating = "REPEATING";
    }

    public static class MediaStatus
    {
        public const string Finished = "FINISHED";

        public const string Releasing = "RELEASING";

        public const string Unreleased = "NOT_YET_RELEASED";

        public const string Cancelled = "CANCELLED";

        public const string Hiatus = "HIATUS";
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
}
