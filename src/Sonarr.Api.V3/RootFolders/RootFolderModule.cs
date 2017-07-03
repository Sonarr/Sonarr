using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace Sonarr.Api.V3.RootFolders
{
    public class RootFolderModule : SonarrRestModuleWithSignalR<RootFolderResource, RootFolder>
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderModule(IRootFolderService rootFolderService,
                                IBroadcastSignalRMessage signalRBroadcaster,
                                RootFolderValidator rootFolderValidator,
                                PathExistsValidator pathExistsValidator,
                                MappedNetworkDriveValidator mappedNetworkDriveValidator)
            : base(signalRBroadcaster)
        {
            _rootFolderService = rootFolderService;

            GetResourceAll = GetRootFolders;
            GetResourceById = GetRootFolder;
            CreateResource = CreateRootFolder;
            DeleteResource = DeleteFolder;

            SharedValidator.RuleFor(c => c.Path)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(pathExistsValidator);
        }

        private RootFolderResource GetRootFolder(int id)
        {
            return _rootFolderService.Get(id).ToResource();
        }

        private int CreateRootFolder(RootFolderResource rootFolderResource)
        {
            var model = rootFolderResource.ToModel();

            return _rootFolderService.Add(model).Id;
        }

        private List<RootFolderResource> GetRootFolders()
        {
            return _rootFolderService.AllWithUnmappedFolders().ToResource();
        }

        private void DeleteFolder(int id)
        {
            _rootFolderService.Remove(id);
        }
    }
}