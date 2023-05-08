using System.Collections.Generic;

namespace NzbDrone.Core.AutoTagging
{
    public class AutoTaggingChanges
    {
        public HashSet<int> TagsToAdd { get; set; }
        public HashSet<int> TagsToRemove { get; set; }

        public AutoTaggingChanges()
        {
            TagsToAdd = new HashSet<int>();
            TagsToRemove = new HashSet<int>();
        }
    }
}
