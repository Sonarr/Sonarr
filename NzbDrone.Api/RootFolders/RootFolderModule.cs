using System;
using System.Collections.Generic;
using NzbDrone.Core.RootFolders;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderModule : NzbDroneRestModule<RootFolderResource>
    {
        private readonly IRootFolderService _rootFolderService;

        public RootFolderModule(IRootFolderService rootFolderService)
        {
            _rootFolderService = rootFolderService;

            GetResourceAll = GetRootFolders;
            GetResourceById = GetRootFolder;
            CreateResource = CreateRootFolder;
            DeleteResource = DeleteFolder;
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