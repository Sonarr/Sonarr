using System;
using System.Collections.Generic;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class MissingEpisodeModel
    {
        public int SeriesId { get; set; }
        public int EpisodeId { get; set; }
        public string SeriesTitle { get; set; }
        public string SeriesTitleSorter { get; set; }
        public string EpisodeNumbering { get; set; }
        public string EpisodeTitle { get; set; }
        public string AirDateSorter { get; set; }
        public string AirDate { get; set; }
        public string Overview { get; set; }
        public string Details { get; set; }
    }
}