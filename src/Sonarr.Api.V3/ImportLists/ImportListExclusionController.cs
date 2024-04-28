using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.ImportLists.Exclusions;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListExclusionController : RestController<ImportListExclusionResource>
    {
        private readonly IImportListExclusionService _importListExclusionService;

        public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                             ImportListExclusionExistsValidator importListExclusionExistsValidator)
        {
            _importListExclusionService = importListExclusionService;

            SharedValidator.RuleFor(c => c.TvdbId).NotEmpty().SetValidator(importListExclusionExistsValidator);
            SharedValidator.RuleFor(c => c.Title).NotEmpty();
        }

        protected override ImportListExclusionResource GetResourceById(int id)
        {
            return _importListExclusionService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        [Obsolete("Deprecated")]
        public List<ImportListExclusionResource> GetImportListExclusions()
        {
            return _importListExclusionService.All().ToResource();
        }

        [HttpGet("paged")]
        [Produces("application/json")]
        public PagingResource<ImportListExclusionResource> GetImportListExclusionsPaged([FromQuery] PagingRequestResource paging)
        {
            var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
            var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>();

            return pageSpec.ApplyToPage(_importListExclusionService.Paged, ImportListExclusionResourceMapper.ToResource);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> AddImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            var importListExclusion = _importListExclusionService.Add(resource.ToModel());

            return Created(importListExclusion.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<ImportListExclusionResource> UpdateImportListExclusion([FromBody] ImportListExclusionResource resource)
        {
            _importListExclusionService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteImportListExclusionResource(int id)
        {
            _importListExclusionService.Delete(id);
        }
    }
}
