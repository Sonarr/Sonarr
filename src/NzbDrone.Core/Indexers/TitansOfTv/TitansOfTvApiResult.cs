using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvApiResult
    {
        public string code { get; set; }
        public int http_code { get; set; }
        public int total { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public List<TitansOfTvTorrent> results { get; set; }
    }

    public class TitansOfTvTorrent
    {
        public string id { get; set; }
        public string series_id { get; set; }
        public string episode_id { get; set; }
        public string season_id { get; set; }
        public int? seeders { get; set; }
        public int? leechers { get; set; }
        public long size { get; set; }
        public int? snatched { get; set; }
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
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string season { get; set; }
        public string episode { get; set; }
        public string series { get; set; }
        public string network { get; set; }
        public string mediainfo { get; set; }
        public string download { get; set; }
        public string additional { get; set; }
        public string episodeUrl { get; set; }
        public string commentUrl { get; set; }
    }
}
