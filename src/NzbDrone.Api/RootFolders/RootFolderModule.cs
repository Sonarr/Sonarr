using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.RootFolders;
using NzbDrone.Api.Mapping;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderModule : NzbDroneRestModuleWithSignalR<RootFolderResource, RootFolder>
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderModule(IRootFolderService rootFolderService,
                                ICommandExecutor commandExecutor,
                                RootFolderValidator rootFolderValidator,
                                PathExistsValidator pathExistsValidator,
                                DroneFactoryValidator droneFactoryValidator)
            : base(commandExecutor)
        {
            _rootFolderService = rootFolderService;

            GetResourceAll = GetRootFolders;
            GetResourceById = GetRootFolder;
            CreateResource = CreateRootFolder;
            DeleteResource = DeleteFolder;

            SharedValidator.RuleFor(c => c.Path).IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(pathExistsValidator)
                           .SetValidator(droneFactoryValidator);
        }

        private RootFolderResource GetRootFolder(int id)
        {
            return _rootFolderService.Get(id).InjectTo<RootFolderResource>();
        }

        private int CreateRootFolder(RootFolderResource rootFolderResource)
        {
            return GetNewId<RootFolder>(_rootFolderService.Add, rootFolderResource);            
        }

        private List<RootFolderResource> GetRootFolders()
        {
            return ToListResource(_rootFolderService.AllWithUnmappedFolders);
        }

        private void DeleteFolder(int id)
        {
            _rootFolderService.Remove(id);
        }
    }
}