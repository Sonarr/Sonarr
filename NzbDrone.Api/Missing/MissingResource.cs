using System;
using System.Linq;

namespace NzbDrone.Api.Missing
{
    public class MissingResource
    {
        public Int32 SeriesId { get; set; }
        public String SeriesTitle { get; set; }
        public Int32 EpisodeId { get; set; }
        public String EpisodeTitle { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32 EpisodeNumber { get; set; }
        public DateTime? AirDate { get; set; }
        public String Overview { get; set; }
    }
}