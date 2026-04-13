using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFilters;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.CustomFilters;

[V5ApiController]
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
    public Ok<List<CustomFilterResource>> GetCustomFilters()
    {
        return TypedResults.Ok(_customFilterService.All().ToResource());
    }

    [RestPostById]
    [Consumes("application/json")]
    public Results<Created<CustomFilterResource>, NotFound> AddCustomFilter([FromBody] CustomFilterResource resource)
    {
        var customFilter = _customFilterService.Add(resource.ToModel());

        return TypedCreated(customFilter.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    public Results<Accepted<CustomFilterResource>, NotFound> UpdateCustomFilter([FromBody] CustomFilterResource resource)
    {
        _customFilterService.Update(resource.ToModel());
        return TypedAccepted(resource.Id);
    }

    [RestDeleteById]
    public NoContent DeleteCustomResource(int id)
    {
        _customFilterService.Delete(id);

        return TypedResults.NoContent();
    }
}
