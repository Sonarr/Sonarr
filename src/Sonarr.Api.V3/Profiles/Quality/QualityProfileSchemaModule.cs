using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public class QualityProfileSchemaModule : SonarrRestModule<QualityProfileResource>
    {
        private readonly IProfileService _profileService;

        public QualityProfileSchemaModule(IProfileService profileService)
            : base("/qualityprofile/schema")
        {
            _profileService = profileService;
            GetResourceSingle = GetSchema;
        }

        private QualityProfileResource GetSchema()
        {
            var qualityProfile = _profileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
