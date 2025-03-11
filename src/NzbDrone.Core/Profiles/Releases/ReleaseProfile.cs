using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles.Releases
{
    public class ReleaseProfile : ModelBase
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<string> Required { get; set; }
        public List<string> Ignored { get; set; }
        public List<int> IndexerIds { get; set; }
        public HashSet<int> Tags { get; set; }

        public ReleaseProfile()
        {
            Enabled = true;
            Required = new List<string>();
            Ignored = new List<string>();
            IndexerIds = new List<int>();
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
