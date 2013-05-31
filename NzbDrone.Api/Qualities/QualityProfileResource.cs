using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfileResource : RestResource
    {
        public String Name { get; set; }
        public QualityResource Cutoff { get; set; }
        public List<QualityResource> Available { get; set; }
        public List<QualityResource> Allowed { get; set; }
    }

    public class QualityResource : RestResource
    {
        public Int32 Weight { get; set; }
        public String Name { get; set; }
    }
}