using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Web.Models
{
    public class QualityModel
    {
        public List<QualityProfile> Profiles { get; set; }
        public List<QualityProfile> UserProfiles { get; set; }
    }
}