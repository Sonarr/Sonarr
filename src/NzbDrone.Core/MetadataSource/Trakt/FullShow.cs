using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Show
    {
        public string title { get; set; }
        public int year { get; set; }
        public string url { get; set; }
        public int first_aired { get; set; }
        public string first_aired_iso { get; set; }
        public int first_aired_utc { get; set; }
        public string country { get; set; }
        public string overview { get; set; }
        public int runtime { get; set; }
        public string status { get; set; }
        public string network { get; set; }
        public string air_day { get; set; }
        public string air_day_utc { get; set; }
        public string air_time { get; set; }
        public string air_time_utc { get; set; }
        public string certification { get; set; }
        public string imdb_id { get; set; }
        public int tvdb_id { get; set; }
        public int tvrage_id { get; set; }
        public int last_updated { get; set; }
        public string poster { get; set; }
        public bool? ended { get; set; }
        public Images images { get; set; }
        public List<string> genres { get; set; }
        public List<Season> seasons { get; set; }
    }

    public class SearchShow
    {
        public string title { get; set; }
        public int year { get; set; }
        public string url { get; set; }
        public int first_aired { get; set; }
        public string first_aired_iso { get; set; }
        public int first_aired_utc { get; set; }
        public string country { get; set; }
        public string overview { get; set; }
        public int runtime { get; set; }
        public string status { get; set; }
        public string network { get; set; }
        public string air_day { get; set; }
        public string air_day_utc { get; set; }
        public string air_time { get; set; }
        public string air_time_utc { get; set; }
        public string certification { get; set; }
        public string imdb_id { get; set; }
        public int tvdb_id { get; set; }
        public int tvrage_id { get; set; }
        public int last_updated { get; set; }
        public string poster { get; set; }
        public Images images { get; set; }
        public List<string> genres { get; set; }
    }
}