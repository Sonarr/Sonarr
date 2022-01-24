using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.CustomFormats
{
    public class SpecificationMatchesGroup
    {
        public Dictionary<ICustomFormatSpecification, bool> Matches { get; set; }

        public bool DidMatch => !(Matches.Any(m => m.Key.Required && m.Value == false) ||
                                  Matches.All(m => m.Value == false));
    }
}
