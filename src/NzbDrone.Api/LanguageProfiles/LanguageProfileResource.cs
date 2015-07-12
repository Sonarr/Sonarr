using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Languages;

namespace NzbDrone.Api.LanguageProfiles
{
    public class LanguageProfileResource : RestResource
    {
        public string Name { get; set; }
        public Language Cutoff { get; set; }
        public List<ProfileLanguageItemResource> Languages { get; set; }
    }

    public class ProfileLanguageItemResource : RestResource
    {
        public Language Language { get; set; }
        public bool Allowed { get; set; }
    }
}
