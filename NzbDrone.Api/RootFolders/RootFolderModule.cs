using System.Linq;
using Nancy;
using NzbDrone.Api.Extentions;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Api.RootFolders
{
    public class RootDirModule : NzbDroneApiModule
    {
        private readonly RootDirProvider _rootDirProvider;

        public RootDirModule(RootDirProvider rootDirProvider)
            : base("//rootdir")
        {
            _rootDirProvider = rootDirProvider;

            Get["/"] = x => GetRootFolders();
            Post["/"] = x => AddRootFolder();
            Delete["/{id}"] = x => DeleteRootFolder((int)x.id);
        }

        private Response AddRootFolder()
        {
            var dir = _rootDirProvider.Add(Request.Body.FromJson<RootDir>());
            return dir.AsResponse(HttpStatusCode.Created);
        }

        private Response GetRootFolders()
        {
            return _rootDirProvider.AllWithFreeSpace().AsResponse();
        }

        private Response DeleteRootFolder(int folderId)
        {
            _rootDirProvider.Remove(folderId);
            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }
}