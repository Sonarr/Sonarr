using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http;

namespace Sonarr.Api.V5.FileSystem;

[V5ApiController]
public class FileSystemController : Controller
{
    private readonly IFileSystemLookupService _fileSystemLookupService;
    private readonly IDiskProvider _diskProvider;
    private readonly IDiskScanService _diskScanService;

    public FileSystemController(IFileSystemLookupService fileSystemLookupService,
                            IDiskProvider diskProvider,
                            IDiskScanService diskScanService)
    {
        _fileSystemLookupService = fileSystemLookupService;
        _diskProvider = diskProvider;
        _diskScanService = diskScanService;
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<FileSystemResult> GetContents(string? path, bool includeFiles = false, bool allowFoldersWithoutTrailingSlashes = false)
    {
        return TypedResults.Ok(_fileSystemLookupService.LookupContents(path, includeFiles, allowFoldersWithoutTrailingSlashes));
    }

    [HttpGet("type")]
    [Produces("application/json")]
    public Ok<object> GetEntityType(string path)
    {
        if (_diskProvider.FileExists(path))
        {
            return TypedResults.Ok<object>(new { type = "file" });
        }

        // Return folder even if it doesn't exist on disk to avoid leaking anything from the UI about the underlying system
        return TypedResults.Ok<object>(new { type = "folder" });
    }

    [HttpGet("mediafiles")]
    [Produces("application/json")]
    public Ok<IEnumerable<object>> GetMediaFiles(string path)
    {
        if (!_diskProvider.FolderExists(path))
        {
            return TypedResults.Ok(Enumerable.Empty<object>());
        }

        return TypedResults.Ok(_diskScanService.GetVideoFiles(path).Select(object (f) => new
        {
            Path = f,
            RelativePath = path.GetRelativePath(f),
            Name = Path.GetFileName(f)
        }));
    }
}
