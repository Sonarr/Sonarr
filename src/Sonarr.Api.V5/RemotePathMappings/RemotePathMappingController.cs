using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.RemotePathMappings;

[V5ApiController]
public class RemotePathMappingController : RestController<RemotePathMappingResource>
{
    private readonly IRemotePathMappingService _remotePathMappingService;

    public RemotePathMappingController(IRemotePathMappingService remotePathMappingService,
                                   PathExistsValidator pathExistsValidator,
                                   MappedNetworkDriveValidator mappedNetworkDriveValidator)
    {
        _remotePathMappingService = remotePathMappingService;

        SharedValidator.RuleFor(c => c.Host)
            .NotEmpty();

        // We cannot use IsValidPath here, because it's a remote path, possibly other OS.
        SharedValidator.RuleFor(c => c.RemotePath)
            .NotEmpty()
            .Must(remotePath => remotePath?.StartsWith(" ") != true)
            .WithMessage("Remote Path '{PropertyValue}' must not start with a space")
            .Must(remotePath => remotePath?.EndsWith(" ") != true)
            .WithMessage("Remote Path '{PropertyValue}' must not end with a space");

        SharedValidator.RuleFor(c => c.LocalPath)
            .Cascade(CascadeMode.Stop)
            .IsValidPath()
            .SetValidator(mappedNetworkDriveValidator)
            .SetValidator(pathExistsValidator)
            .SetValidator(new SystemFolderValidator())
            .NotEqual("/")
            .WithMessage("Cannot be set to '/'");
    }

    protected override RemotePathMappingResource GetResourceById(int id)
    {
        return _remotePathMappingService.Get(id).ToResource();
    }

    [RestPostById]
    [Consumes("application/json")]
    public Results<Created<RemotePathMappingResource>, NotFound> CreateMapping([FromBody] RemotePathMappingResource resource)
    {
        var model = resource.ToModel();

        return TypedCreated(_remotePathMappingService.Add(model).Id);
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<RemotePathMappingResource>> GetMappings()
    {
        return TypedResults.Ok(_remotePathMappingService.All().ToResource());
    }

    [RestDeleteById]
    public NoContent DeleteMapping(int id)
    {
        _remotePathMappingService.Remove(id);

        return TypedResults.NoContent();
    }

    [RestPutById]
    public Results<Ok<RemotePathMappingResource>, NotFound> UpdateMapping([FromBody] RemotePathMappingResource resource)
    {
        var mapping = resource.ToModel();

        return TypedResults.Ok(_remotePathMappingService.Update(mapping).ToResource());
    }
}
