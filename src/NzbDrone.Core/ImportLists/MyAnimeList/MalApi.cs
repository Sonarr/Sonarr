using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.MyAnimeList
{
    public class MalResponse
    {
        [JsonProperty("data")]
        public List<MalAnime> Animes { get; set; }
    }

    public class MalAnime
    {
        [JsonProperty("node")]
        public MalAnimeInfo AnimeInfo { get; set; }

        [JsonProperty("list_status")]
        public MalAnimeListStatus ListStatus { get; set; }
    }

    public class MalAnimeInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class MalAnimeListStatus
    {
        public string Status { get; set; }
    }
}
