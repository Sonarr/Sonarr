using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class UpcomingEpisodesModel
    {
        public List<UpcomingEpisodeModel> Yesterday { get; set; }
        public List<UpcomingEpisodeModel> Today { get; set; }
        public List<UpcomingEpisodeModel> Tomorrow { get; set; }
        public List<UpcomingEpisodeModel> TwoDays { get; set; }
        public List<UpcomingEpisodeModel> ThreeDays { get; set; }
        public List<UpcomingEpisodeModel> FourDays { get; set; }
        public List<UpcomingEpisodeModel> FiveDays { get; set; }
        public List<UpcomingEpisodeModel> SixDays { get; set; }
        public List<UpcomingEpisodeModel> Later { get; set; }
    }
}