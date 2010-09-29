using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Repository
{
    public class QualityProfile
    {
        public int Id { get; set; }
        public Quality Cutoff { get; set; }
        public string Qualitys { get; set; }
        public Quality[] Q { get; set; }
    }
}
