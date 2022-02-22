using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Profiles.Languages
{
    public class LanguageProfile : ModelBase
    {
        public string Name { get; set; }
        public List<LanguageProfileItem> Languages { get; set; }
        public bool UpgradeAllowed { get; set;  }
        public Language Cutoff { get; set;  }

        public Language FirstAllowedLanguage()
        {
            return Languages.First(q => q.Allowed).Language;
        }

        public Language LastAllowedLanguage()
        {
            return Languages.Last(q => q.Allowed).Language;
        }
    }
}
