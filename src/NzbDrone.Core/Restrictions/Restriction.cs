using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Restrictions
{
    public class Restriction : ModelBase
    {
        public String Required { get; set; }
        public String Preferred { get; set; }
        public String Ignored { get; set; }
        public HashSet<Int32> Tags { get; set; }

        public Restriction()
        {
            Tags = new HashSet<Int32>();
        }
    }
}
