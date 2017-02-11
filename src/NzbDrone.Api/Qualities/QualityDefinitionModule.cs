using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Qualities
{
    public class QualityDefinitionModule : SonarrRestModule<QualityDefinitionResource>
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
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
        }

        private QualityDefinitionResource GetById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        private List<QualityDefinitionResource> GetAll()
        {
            return _qualityDefinitionService.All().ToResource();
        }
    }
}