using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Qualities
{
    public class QualityDefinitionModule : NzbDroneRestModule<QualityDefinitionResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionModule(IQualityDefinitionService qualityDefinitionService)
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;

            GetResourceById = GetById;

            UpdateResource = Update;
        }

        private void Update(QualityDefinitionResource resource)
        {
            var model = resource.InjectTo<QualityDefinition>();
            _qualityDefinitionService.Update(model);
        }

        private QualityDefinitionResource GetById(int id)
        {
            return _qualityDefinitionService.Get((Quality)id).InjectTo<QualityDefinitionResource>();
        }

        private List<QualityDefinitionResource> GetAll()
        {
            return ToListResource(_qualityDefinitionService.All);
        }
    }
}