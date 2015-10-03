using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Restrictions
{
    public class RestrictionResource : RestResource
    {
        public string Required { get; set; }
        public string Preferred { get; set; }
        public string Ignored { get; set; }
        public HashSet<int> Tags { get; set; }

        public RestrictionResource()
        {
            Tags = new HashSet<int>();
        }
    }
}
