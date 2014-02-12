using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;
using System.Linq;

namespace NzbDrone.Api.Qualities
{
    public class QualityProfileSchemaModule : NzbDroneRestModule<QualityProfileResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityProfileSchemaModule(IQualityDefinitionService qualityDefinitionService)
            : base("/qualityprofile/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
        }

        private List<QualityProfileResource> GetAll()
        {
            var items = _qualityDefinitionService.All()
                .OrderBy(v => v.Weight)
                .Select(v => new QualityProfileItem { Quality = v.Quality, Allowed = false })
                .ToList();

            var profile = new QualityProfile();
            profile.Cutoff = Quality.Unknown;
            profile.Items = items;

            return new List<QualityProfileResource> { profile.InjectTo<QualityProfileResource>() };
        }
    }
}