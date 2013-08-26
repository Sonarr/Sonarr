using System.Collections.Generic;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderModule : NzbDroneRestModule<RootFolderResource>
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderModule(IRootFolderService rootFolderService)
        {
            _rootFolderService = rootFolderService;

            GetResourceAll = GetRootFolders;
            CreateResource = CreateRootFolder;
            DeleteResource = DeleteFolder;
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