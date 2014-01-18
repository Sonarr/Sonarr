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
            : base("/qualityprofiles/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
        }

        private List<QualityProfileResource> GetAll()
        {
            var profile = new QualityProfile();
            profile.Cutoff = Quality.Unknown;
            profile.Allowed = new List<Quality>();

            return new List<QualityProfileResource> { QualityToResource(profile) };
        }

        private QualityProfileResource QualityToResource(QualityProfile profile)
        {
            return new QualityProfileResource
            {
                Cutoff = QualityToResource(_qualityDefinitionService.Get(profile.Cutoff)),
                Available = _qualityDefinitionService.All().Select(QualityToResource).ToList(),
                Allowed = profile.Allowed.Select(_qualityDefinitionService.Get).Select(QualityToResource).ToList(),
                Name = profile.Name,
                Id = profile.Id
            };
        }


        private QualityResource QualityToResource(QualityDefinition config)
        {
            return new QualityResource
            {
                Id = config.Quality.Id,
                Name = config.Quality.Name,
                Weight = config.Weight
            };
        }
    }
}