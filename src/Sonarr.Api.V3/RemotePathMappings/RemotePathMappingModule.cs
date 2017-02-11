using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;

namespace Sonarr.Api.V3.RemotePathMappings
{
    public class RemotePathMappingModule : SonarrRestModule<RemotePathMappingResource>
    {
        private readonly IRemotePathMappingService _remotePathMappingService;

        public RemotePathMappingModule(IRemotePathMappingService remotePathMappingService,
                                       PathExistsValidator pathExistsValidator,
                                       MappedNetworkDriveValidator mappedNetworkDriveValidator)
        {
            _remotePathMappingService = remotePathMappingService;

            GetResourceAll = GetMappings;
            GetResourceById = GetMappingById;
            CreateResource = CreateMapping;
            DeleteResource = DeleteMapping;
            UpdateResource = UpdateMapping;

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

        private RemotePathMappingResource GetMappingById(int id)
        {
            return _remotePathMappingService.Get(id).ToResource();
        }

        private int CreateMapping(RemotePathMappingResource resource)
        {
            var model = resource.ToModel();

            return _remotePathMappingService.Add(model).Id;
        }

        private List<RemotePathMappingResource> GetMappings()
        {
            return _remotePathMappingService.All().ToResource();
        }

        private void DeleteMapping(int id)
        {
            _remotePathMappingService.Remove(id);
        }

        private void UpdateMapping(RemotePathMappingResource resource)
        {
            var mapping = resource.ToModel();

            _remotePathMappingService.Update(mapping);
        }
    }
}