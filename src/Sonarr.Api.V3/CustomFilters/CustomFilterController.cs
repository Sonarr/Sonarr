using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFilters;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.CustomFilters
{
    [V3ApiController]
    public class CustomFilterController : RestController<CustomFilterResource>
    {
        private readonly ICustomFilterService _customFilterService;

        public CustomFilterController(ICustomFilterService customFilterService)
        {
            _customFilterService = customFilterService;
        }

        protected override CustomFilterResource GetResourceById(int id)
        {
            return _customFilterService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<CustomFilterResource> GetCustomFilters()
        {
            return _customFilterService.All().ToResource();
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<CustomFilterResource> AddCustomFilter([FromBody] CustomFilterResource resource)
        {
            var customFilter = _customFilterService.Add(resource.ToModel());

            return Created(customFilter.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<CustomFilterResource> UpdateCustomFilter([FromBody] CustomFilterResource resource)
        {
            _customFilterService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteCustomResource(int id)
        {
            _customFilterService.Delete(id);
        }
    }
}
