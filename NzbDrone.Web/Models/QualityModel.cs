using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class QualityModel
    {
        public List<QualityProfile> Profiles { get; set; }
        public List<QualityProfile> UserProfiles { get; set; }
        //public List<QualityTypes> Qualities { get; set; }

        [DisplayName("Default Quality Profile")]
        public int DefaultProfileId { get; set; }

        public SelectList SelectList { get; set; }
    }
}