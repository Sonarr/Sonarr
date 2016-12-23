using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Restrictions
{
    public class Restriction : ModelBase
    {
        public string Required { get; set; }
        public string Preferred { get; set; }
        public string Ignored { get; set; }
        public HashSet<int> Tags { get; set; }

        public Restriction()
        {
            Tags = new HashSet<int>();
        }
    }
}
