using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Sonarr.Http;
using Workarr.Disk;
using Workarr.Extensions;
using Workarr.MediaFiles;

namespace Sonarr.Api.V3.FileSystem
{
    [V3ApiController]
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
        public IActionResult GetContents(string path, bool includeFiles = false, bool allowFoldersWithoutTrailingSlashes = false)
        {
            return Ok(_fileSystemLookupService.LookupContents(path, includeFiles, allowFoldersWithoutTrailingSlashes));
        }

        [HttpGet("type")]
        [Produces("application/json")]
        public object GetEntityType(string path)
        {
            if (_diskProvider.FileExists(path))
            {
                return new { type = "file" };
            }

            // Return folder even if it doesn't exist on disk to avoid leaking anything from the UI about the underlying system
            return new { type = "folder" };
        }

        [HttpGet("mediafiles")]
        [Produces("application/json")]
        public object GetMediaFiles(string path)
        {
            if (!_diskProvider.FolderExists(path))
            {
                return Array.Empty<string>();
            }

            return _diskScanService.GetVideoFiles(path).Select(f => new
            {
                Path = f,
                RelativePath = path.GetRelativePath(f),
                Name = Path.GetFileName(f)
            });
        }
    }
}
