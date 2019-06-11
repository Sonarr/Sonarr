using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles.Releases
{
    public class ReleaseProfile : ModelBase
    {
        public bool Enabled { get; set; }
        public string Required { get; set; }
        public string Ignored { get; set; }
        public List<KeyValuePair<string, int>> Preferred { get; set; }
        public bool IncludePreferredWhenRenaming { get; set; }
        public int IndexerId { get; set; }
        public HashSet<int> Tags { get; set; }

        public ReleaseProfile()
        {
            Enabled = true;
            Preferred = new List<KeyValuePair<string, int>>();
            IncludePreferredWhenRenaming = true;
            Tags = new HashSet<int>();
            IndexerId = 0;
        }
    }
}
