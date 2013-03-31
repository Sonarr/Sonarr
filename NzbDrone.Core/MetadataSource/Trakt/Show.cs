using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Show
    {
        public string title { get; set; }
        public int year { get; set; }
        public string url { get; set; }
        public DateTime? first_aired { get; set; }
        public string country { get; set; }
        public string overview { get; set; }
        public int runtime { get; set; }
        public string status { get; set; }
        public string network { get; set; }
        public string air_day { get; set; }
        public string air_time { get; set; }
        public string certification { get; set; }
        public string imdb_id { get; set; }
        public int tvdb_id { get; set; }
        public int tvrage_id { get; set; }
        public int last_updated { get; set; }
        public string poster { get; set; }
        public Images images { get; set; }
        public List<TopWatcher> top_watchers { get; set; }
        public List<TopEpisode> top_episodes { get; set; }
        public Ratings ratings { get; set; }
        public Stats stats { get; set; }
        public People people { get; set; }
        public List<string> genres { get; set; }
        public List<Season> seasons { get; set; }
    }
}