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
        public List<UpcomingEpisodeModel> Week { get; set; }
    }
}