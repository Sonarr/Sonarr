using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Profiles.Languages
{
    public class LanguageProfile : ModelBase
    {
        public string Name { get; set; }
        public List<ProfileLanguageItem> Languages { get; set; }
        public Language Cutoff { get; set;  }

        public Language LastAllowedLanguage()
        {
            return Languages.Last(q => q.Allowed).Language;
        }
    }
}
