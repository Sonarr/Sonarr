using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Model;

namespace NzbDrone.Api.Upcoming
{
    public class UpcomingResource
    {
        public Int32 SeriesId { get; set; }
        public String SeriesTitle { get; set; }

        public Int32 EpisodeId { get; set; }
        public String EpisodeTitle { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32 EpisodeNumber { get; set; }
        public DateTime? AirTime { get; set; }
        public Int32 Status { get; set; }
        public String Overview { get; set; }
    }
}
