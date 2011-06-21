using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
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

        public IList<int> Seasons { get; set; }

        //View & Edit
        [DisplayName("Path")]
        public string Path { get; set; }

        [DisplayName("Quality Profile")]
        public virtual int QualityProfileId { get; set; }

        //Editing Only
        [DisplayName("Use Season Folder")]
        public bool SeasonFolder { get; set; }

        [DisplayName("Monitored")]
        public bool Monitored { get; set; }

        [DisplayName("Season Editor")]
        public List<SeasonEditModel> SeasonEditor { get; set; }
    }
}