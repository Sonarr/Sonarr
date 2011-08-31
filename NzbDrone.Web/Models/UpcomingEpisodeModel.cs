using System;

namespace NzbDrone.Web.Models
{
    public class UpcomingEpisodeModel
    {
        public int SeriesId { get; set; }
        public int EpisodeId { get; set; }
        public string SeriesTitle { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime AirDate { get; set; }
    }
}