using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.RemotePathMappings
{
    [V3ApiController]
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
                           .NotEmpty();

            SharedValidator.RuleFor(c => c.LocalPath)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(pathExistsValidator);
        }

        protected override RemotePathMappingResource GetResourceById(int id)
        {
            return _remotePathMappingService.Get(id).ToResource();
        }

        [RestPostById]
        public ActionResult<RemotePathMappingResource> CreateMapping(RemotePathMappingResource resource)
        {
            var model = resource.ToModel();

            return Created(_remotePathMappingService.Add(model).Id);
        }

        [HttpGet]
        public List<RemotePathMappingResource> GetMappings()
        {
            return _remotePathMappingService.All().ToResource();
        }

        [RestDeleteById]
        public void DeleteMapping(int id)
        {
            _remotePathMappingService.Remove(id);
        }

        [RestPutById]
        public ActionResult<RemotePathMappingResource> UpdateMapping(RemotePathMappingResource resource)
        {
            var mapping = resource.ToModel();

            return Accepted(_remotePathMappingService.Update(mapping));
        }
    }
}
