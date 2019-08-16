using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles.Releases
{
    public class ReleaseProfile : ModelBase
    {
        public string Required { get; set; }
        public string Ignored { get; set; }
        public List<KeyValuePair<string, int>> Preferred { get; set; }
        public bool IncludePreferredWhenRenaming { get; set; }
        public HashSet<int> Tags { get; set; }

        public ReleaseProfile()
        {
            Preferred = new List<KeyValuePair<string, int>>();
            IncludePreferredWhenRenaming = true;
            Tags = new HashSet<int>();
        }
    }

    public class ReleaseProfilePreferredComparer : IComparer<KeyValuePair<string, int>>
    {
        public int Compare(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
        {
            return y.Value.CompareTo(x.Value);
        }
    }
}
