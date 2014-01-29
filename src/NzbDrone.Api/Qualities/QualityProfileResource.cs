using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfileResource : RestResource
    {
        public String Name { get; set; }
        public Quality Cutoff { get; set; }
        public List<QualityProfileItemResource> Items { get; set; }
    }

    public class QualityProfileItemResource : RestResource
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }
}