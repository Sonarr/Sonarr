using Workarr.CustomFormats.Specifications;

namespace Workarr.CustomFormats
{
    public class SpecificationMatchesGroup
    {
        public Dictionary<ICustomFormatSpecification, bool> Matches { get; set; }

        public bool DidMatch => !(Matches.Any(m => m.Key.Required && m.Value == false) ||
                                  Matches.All(m => m.Value == false));
    }
}
