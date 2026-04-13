using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ImportLists.Exclusions;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.ImportLists;

[V5ApiController]
public class ImportListExclusionController : RestController<ImportListExclusionResource>
{
    private readonly IImportListExclusionService _importListExclusionService;

    public ImportListExclusionController(IImportListExclusionService importListExclusionService,
                                         ImportListExclusionExistsValidator importListExclusionExistsValidator)
    {
        _importListExclusionService = importListExclusionService;

        SharedValidator.RuleFor(c => c.TvdbId).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .SetValidator(importListExclusionExistsValidator);

        SharedValidator.RuleFor(c => c.Title).NotEmpty();
    }

    protected override ImportListExclusionResource GetResourceById(int id)
    {
        return _importListExclusionService.Get(id).ToResource();
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<PagingResource<ImportListExclusionResource>> GetImportListExclusions([FromQuery] PagingRequestResource paging)
    {
        var pagingResource = new PagingResource<ImportListExclusionResource>(paging);
        var pageSpec = pagingResource.MapToPagingSpec<ImportListExclusionResource, ImportListExclusion>(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "id",
                "title",
                "tvdbId"
            },
            "id",
            SortDirection.Descending);

        return TypedResults.Ok(pageSpec.ApplyToPage(_importListExclusionService.Paged, ImportListExclusionResourceMapper.ToResource));
    }

    [RestPostById]
    [Consumes("application/json")]
    public Results<Created<ImportListExclusionResource>, NotFound> AddImportListExclusion([FromBody] ImportListExclusionResource resource)
    {
        var importListExclusion = _importListExclusionService.Add(resource.ToModel());

        return TypedCreated(importListExclusion.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    public Results<Accepted<ImportListExclusionResource>, NotFound> UpdateImportListExclusion([FromBody] ImportListExclusionResource resource)
    {
        _importListExclusionService.Update(resource.ToModel());

        return TypedAccepted(resource.Id);
    }

    [RestDeleteById]
    public NoContent DeleteImportListExclusion(int id)
    {
        _importListExclusionService.Delete(id);

        return TypedResults.NoContent();
    }

    [HttpDelete("bulk")]
    [Consumes("application/json")]
    public NoContent DeleteImportListExclusions([FromBody] ImportListExclusionBulkResource resource)
    {
        _importListExclusionService.Delete(resource.Ids.ToList());

        return TypedResults.NoContent();
    }
}
