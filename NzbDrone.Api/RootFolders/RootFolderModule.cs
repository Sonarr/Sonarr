using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderModule : NzbDroneApiModule
    {
        private readonly RootFolderService _rootFolderService;

        public RootFolderModule(RootFolderService rootFolderService)
            : base("/rootfolder")
        {
            _rootFolderService = rootFolderService;

            Get["/"] = x => GetRootFolders();
            Post["/"] = x => AddRootFolder();
            Delete["/{id}"] = x => DeleteRootFolder((int)x.id);
        }

        private Response AddRootFolder()
        {
            var dir = _rootFolderService.Add(Request.Body.FromJson<RootFolder>());
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