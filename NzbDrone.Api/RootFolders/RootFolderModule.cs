using System.Linq;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.RootFolders
{
    public class RootDirModule : NzbDroneApiModule
    {
        private readonly RootFolderService _rootFolderService;

        public RootDirModule(RootFolderService rootFolderService)
            : base("//rootdir")
        {
            _rootFolderService = rootFolderService;

            Get["/"] = x => GetRootFolders();
            Post["/"] = x => AddRootFolder();
            Delete["/{id}"] = x => DeleteRootFolder((int)x.id);
        }

        private Response AddRootFolder()
        {
            var dir = _rootFolderService.Add(Request.Body.FromJson<RootDir>());
            return dir.AsResponse(HttpStatusCode.Created);
        }

        private Response GetRootFolders()
        {
            return _rootFolderService.All().AsResponse();
        }

        private Response DeleteRootFolder(int folderId)
        {
            _rootFolderService.Remove(folderId);
            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }
}