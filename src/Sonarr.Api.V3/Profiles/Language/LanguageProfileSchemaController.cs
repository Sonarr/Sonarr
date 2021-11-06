using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Language
{
    [V3ApiController("languageprofile/schema")]
    public class LanguageProfileSchemaController : Controller
    {
        private readonly ILanguageProfileService _profileService;

        public LanguageProfileSchemaController(ILanguageProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public LanguageProfileResource GetSchema()
        {
            var qualityProfile = _profileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
