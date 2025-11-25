using NzbDrone.Common.Extensions;
using NzbDrone.Core.RootFolders;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.RootFolders;

public class RootFolderResource : RestResource
{
    public string? Path { get; set; }
    public bool Accessible { get; set; }
    public bool IsEmpty { get; set; }
    public long? FreeSpace { get; set; }
    public long? TotalSpace { get; set; }

    public List<UnmappedFolder> UnmappedFolders { get; set; } = [];
}

public static class RootFolderResourceMapper
{
    public static RootFolderResource ToResource(this RootFolder model)
    {
        return new RootFolderResource
        {
            Id = model.Id,
            Path = model.Path.GetCleanPath(),
            Accessible = model.Accessible,
            IsEmpty = model.IsEmpty,
            FreeSpace = model.FreeSpace,
            TotalSpace = model.TotalSpace,
            UnmappedFolders = model.UnmappedFolders
        };
    }

    public static RootFolder ToModel(this RootFolderResource resource)
    {
        return new RootFolder
        {
            Id = resource.Id,

            Path = resource.Path

            // Accessible
            // IsEmpty
            // FreeSpace
            // UnmappedFolders
        };
    }

    public static List<RootFolderResource> ToResource(this IEnumerable<RootFolder> models)
    {
        return models.Select(ToResource).ToList();
    }
}
