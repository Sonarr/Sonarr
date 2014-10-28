using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Restrictions
{
    public class RestrictionResource : RestResource
    {
        public String Required { get; set; }
        public String Preferred { get; set; }
        public String Ignored { get; set; }
        public HashSet<Int32> Tags { get; set; }

        public RestrictionResource()
        {
            Tags = new HashSet<Int32>();
        }
    }
}
