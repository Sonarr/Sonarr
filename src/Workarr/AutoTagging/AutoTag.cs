using Workarr.AutoTagging.Specifications;
using Workarr.Datastore;

namespace Workarr.AutoTagging
{
    public class AutoTag : ModelBase
    {
        public AutoTag()
        {
            Tags = new HashSet<int>();
        }

        public string Name { get; set; }
        public List<IAutoTaggingSpecification> Specifications { get; set; }
        public bool RemoveTagsAutomatically { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}
