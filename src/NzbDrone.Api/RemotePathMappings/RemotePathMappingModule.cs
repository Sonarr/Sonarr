using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Api.Mapping;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation.Paths;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Config
{
    public class RemotePathMappingModule : NzbDroneRestModule<RemotePathMappingResource>
    {
        private readonly IRemotePathMappingService _remotePathMappingService;

        public RemotePathMappingModule(IConfigService configService, IRemotePathMappingService remotePathMappingService, PathExistsValidator pathExistsValidator)
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
                           .SetValidator(pathExistsValidator);
        }

        private RemotePathMappingResource GetMappingById(int id)
        {
            return _remotePathMappingService.Get(id).InjectTo<RemotePathMappingResource>();
        }

        private int CreateMapping(RemotePathMappingResource rootFolderResource)
        {
            return GetNewId<RemotePathMapping>(_remotePathMappingService.Add, rootFolderResource);
        }

        private List<RemotePathMappingResource> GetMappings()
        {
            return ToListResource(_remotePathMappingService.All);
        }

        private void DeleteMapping(int id)
        {
            _remotePathMappingService.Remove(id);
        }

        private void UpdateMapping(RemotePathMappingResource resource)
        {
            var mapping = _remotePathMappingService.Get(resource.Id);

            mapping.InjectFrom(resource);

            _remotePathMappingService.Update(mapping);
        }
    }
}