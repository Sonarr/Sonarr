using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MyAnimeListResponse
    {
        [JsonProperty("data")]
        public List<MyAnimeListItem> Animes { get; set; }
    }

    public class MyAnimeListItem
    {
        [JsonProperty("node")]
        public MyAnimeListItemInfo AnimeListInfo { get; set; }

        [JsonProperty("list_status")]
        public MyAnimeListStatusResult ListStatus { get; set; }
    }

    public class MyAnimeListStatusResult
    {
        public string Status { get; set; }
    }

    public class MyAnimeListItemInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class MyAnimeListIds
    {
        [JsonProperty("mal_id")]
        public int MalId { get; set; }

        [JsonProperty("thetvdb_id")]
        public int TvdbId { get; set; }
    }

    public class MyAnimeListAuthToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
