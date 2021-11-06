using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Qualities;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Qualities
{
    [V3ApiController]
    public class QualityDefinitionController : RestController<QualityDefinitionResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionController(IQualityDefinitionService qualityDefinitionService)
        {
            _qualityDefinitionService = qualityDefinitionService;
        }

        [RestPutById]
        public ActionResult<QualityDefinitionResource> Update(QualityDefinitionResource resource)
        {
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
            return Accepted(model.Id);
        }

        protected override QualityDefinitionResource GetResourceById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<QualityDefinitionResource> GetAll()
        {
            return _qualityDefinitionService.All().ToResource();
        }

        [HttpPut("update")]
        public object UpdateMany([FromBody] List<QualityDefinitionResource> resource)
        {
            //Read from request
            var qualityDefinitions = resource
                                                 .ToModel()
                                                 .ToList();

            _qualityDefinitionService.UpdateMany(qualityDefinitions);

            return Accepted(_qualityDefinitionService.All()
                .ToResource());
        }
    }
}
