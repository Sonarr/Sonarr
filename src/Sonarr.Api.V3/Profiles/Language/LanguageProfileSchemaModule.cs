using System.Linq;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Language
{
    public class LanguageProfileSchemaModule : SonarrRestModule<LanguageProfileResource>
    {

        public LanguageProfileSchemaModule()
            : base("/languageprofile/schema")
        {
            GetResourceSingle = GetAll;
        }

        private LanguageProfileResource GetAll()
        {
            var orderedLanguages = NzbDrone.Core.Languages.Language.All
                                           .Where(l => l != NzbDrone.Core.Languages.Language.Unknown)
                                           .OrderByDescending(l => l.Name)
                                           .ToList();

            orderedLanguages.Insert(0, NzbDrone.Core.Languages.Language.Unknown);

            var languages = orderedLanguages.Select(v => new ProfileLanguageItem {Language = v, Allowed = false})
                                            .ToList();

            var profile = new LanguageProfile
                          {
                              Cutoff = NzbDrone.Core.Languages.Language.Unknown,
                              Languages = languages
                          };

            return profile.ToResource();
        }
    }
}