using System.Collections.Generic;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Profiles.Languages
{
    public class LanguageProfileResource : RestResource
    {
        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public NzbDrone.Core.Languages.Language Cutoff { get; set; }
        public List<LanguageProfileItemResource> Languages { get; set; }
    }

    public class LanguageProfileItemResource : RestResource
    {
        public NzbDrone.Core.Languages.Language Language { get; set; }
        public bool Allowed { get; set; }
    }
}
