using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Api.QualityProfiles
{
    public class QualityProfileModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public QualityTypes Cutoff { get; set; }
        public List<QualityTypes> Allowed { get; set; }
    }
}
