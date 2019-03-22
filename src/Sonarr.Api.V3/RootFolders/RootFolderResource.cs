using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.RootFolders;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.RootFolders
{
    public class RootFolderResource : RestResource
    {
        public string Path { get; set; }
        public long? FreeSpace { get; set; }

        public List<UnmappedFolder> UnmappedFolders { get; set; }
    }

    public static class RootFolderResourceMapper
    {
        public static RootFolderResource ToResource(this RootFolder model)
        {
            if (model == null) return null;

            return new RootFolderResource
            {
                Id = model.Id,

                Path = model.Path,
                FreeSpace = model.FreeSpace,
                UnmappedFolders = model.UnmappedFolders
            };
        }

        public static RootFolder ToModel(this RootFolderResource resource)
        {
            if (resource == null) return null;

            return new RootFolder
            {
                Id = resource.Id,

                Path = resource.Path,
                //FreeSpace
                //UnmappedFolders
            };
        }

        public static List<RootFolderResource> ToResource(this IEnumerable<RootFolder> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}