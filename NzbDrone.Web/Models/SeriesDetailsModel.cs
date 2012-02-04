using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Web.Models
{
    public class SeriesDetailsModel
    {
        public int SeriesId { get; set; }
        public string Title { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string QualityProfileName { get; set; }
        public string Overview { get; set; }
        public string NextAiring { get; set; }
        public string Path { get; set; }
        public bool HasBanner { get; set; }
        public List<SeasonModel> Seasons { get; set; }
    }
}