using Workarr.AutoTagging.Specifications;

namespace Workarr.AutoTagging
{
    public class SpecificationMatchesGroup
    {
        public Dictionary<IAutoTaggingSpecification, bool> Matches { get; set; }

        public bool DidMatch => !(Matches.Any(m => m.Key.Required && m.Value == false) ||
                                  Matches.All(m => m.Value == false));
    }
}
