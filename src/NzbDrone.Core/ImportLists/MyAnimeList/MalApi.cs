using System.Collections.Generic;
using System.Runtime.Serialization;
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
        public MalAnimeStatus Status { get; set; }
    }

    public enum MalAnimeStatus
    {
        [EnumMember(Value = "watching")]
        Watching,
        [EnumMember(Value = "completed")]
        Completed,
        [EnumMember(Value = "on_hold")]
        OnHold,
        [EnumMember(Value = "dropped")]
        Dropped,
        [EnumMember(Value = "plan_to_watch")]
        PlanToWatch
    }
}
