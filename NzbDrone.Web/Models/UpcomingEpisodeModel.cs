using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class UpcomingEpisodeModel
    {
        public int SeriesId { get; set; }
        public int EpisodeId { get; set; }
        public string SeriesTitle { get; set; }
        public string EpisodeNumbering { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime AirDateTime { get; set; }
        public string AirDate { get; set; }
        public string AirTime { get; set; }
        public string Status { get; set; }
    }
}