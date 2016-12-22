using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Api.LanguageProfiles
{
    public class LanguageProfileSchemaModule : NzbDroneRestModule<LanguageProfileResource>
    {

        public LanguageProfileSchemaModule()
            : base("/languageprofile/schema")
        {
            GetResourceAll = GetAll;
        }

        private List<LanguageProfileResource> GetAll()
        {
            var languages = Language.All
                .OrderBy(v => v.Name)
                .Select(v => new ProfileLanguageItem { Language = v, Allowed = false })
                .ToList();

            var profile = new LanguageProfile();
            profile.Cutoff = Language.Unknown;
            profile.Languages = languages;

            return new List<LanguageProfileResource> { profile.ToResource() };
        }
    }
}