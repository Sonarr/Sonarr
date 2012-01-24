using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class SeriesModel
    {
        public int SeriesId { get; set; }

        //View Only
        public string Title { get; set; }
        public int SeasonsCount { get; set; }
        public int EpisodeCount { get; set; }
        public int EpisodeFileCount { get; set; }
        public string Status { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string QualityProfileName { get; set; }
        public string Overview { get; set; }
        public int Episodes { get; set; }
        public bool HasBanner { get; set; }
        public string NextAiring { get; set; }

        public IList<int> Seasons { get; set; }

        //View & Edit
        [DisplayName("Path")]
        [Description("Where should NzbDrone store episodes for this series?")]
        public string Path { get; set; }

        [DisplayName("Quality Profile")]
        [Description("Which Quality Profile should NzbDrone use to download episodes?")]
        public virtual int QualityProfileId { get; set; }

        //Editing Only
        [DisplayName("Use Season Folder")]
        [Description("Should downloaded episodes be stored in season folders?")]
        public bool SeasonFolder { get; set; }

        [DisplayName("Monitored")]
        [Description("Should NzbDrone download episodes for this series?")]
        public bool Monitored { get; set; }

        [DisplayName("Backlog Status")]
        [Description("Should NzbDrone download past missing episodes?")]
        public int BacklogStatus { get; set; }
    }
}