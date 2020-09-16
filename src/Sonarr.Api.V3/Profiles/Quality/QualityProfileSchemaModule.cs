using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Profiles.Quality
{
    public class QualityProfileSchemaModule : SonarrRestModule<QualityProfileResource>
    {
        private readonly IQualityProfileService _qualityProfileService;

        public QualityProfileSchemaModule(IQualityProfileService qualityProfileService)
            : base("/qualityprofile/schema")
        {
            _qualityProfileService = qualityProfileService;
            GetResourceSingle = GetSchema;
        }

        private QualityProfileResource GetSchema()
        {
            var qualityProfile = _qualityProfileService.GetDefaultProfile(string.Empty);

            return qualityProfile.ToResource();
        }
    }
}
