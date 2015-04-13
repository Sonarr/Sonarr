using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class Result
    {
        public string id { get; set; }
        public string series_id { get; set; }
        public string episode_id { get; set; }
        public string season_id { get; set; }
        public string seeders { get; set; }
        public string leechers { get; set; }
        public string size { get; set; }
        public string snatched { get; set; }
        public int user_id { get; set; }
        public string anonymous { get; set; }
        public string container { get; set; }
        public string codec { get; set; }
        public string source { get; set; }
        public string resolution { get; set; }
        public string origin { get; set; }
        public string language { get; set; }
        public string release_name { get; set; }
        public string tracker_updated_at { get; set; }
        public int created_at { get; set; }
        public int updated_at { get; set; }
        public string season { get; set; }
        public string episode { get; set; }
        public string series { get; set; }
        public string network { get; set; }
        public string download { get; set; }
    }

    public class ApiResult
    {
        public string code { get; set; }
        public int http_code { get; set; }
        public int total { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public List<Result> results { get; set; }
    }
}
