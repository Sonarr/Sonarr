using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model.TvRage
{
    public class TvRageEpisode
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string ProductionCode { get; set; }
        public DateTime AirDate { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }
}
