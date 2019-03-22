using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Language
{
    public class LanguageProfileSchemaModule : SonarrRestModule<LanguageProfileResource>
    {
        private readonly LanguageProfileService _languageProfileService;

        public LanguageProfileSchemaModule(LanguageProfileService languageProfileService)
            : base("/languageprofile/schema")
        {
            _languageProfileService = languageProfileService;
            GetResourceSingle = GetAll;
        }

        private LanguageProfileResource GetAll()
        {
            var profile = _languageProfileService.GetDefaultProfile(string.Empty);
            return profile.ToResource();
        }
    }
}
