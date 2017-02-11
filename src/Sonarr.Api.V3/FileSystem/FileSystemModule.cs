using System;
using System.IO;
using System.Linq;
using Nancy;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.FileSystem
{
    public class FileSystemModule : SonarrV3Module
    {
        private readonly IFileSystemLookupService _fileSystemLookupService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;

        public FileSystemModule(IFileSystemLookupService fileSystemLookupService,
                                IDiskProvider diskProvider,
                                IDiskScanService diskScanService)
            : base("/filesystem")
        {
            _fileSystemLookupService = fileSystemLookupService;
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            Get["/"] = x => GetContents();
            Get["/type"] = x => GetEntityType();
            Get["/mediafiles"] = x => GetMediaFiles();
        }

        private Response GetContents()
        {
            var pathQuery = Request.Query.path;
            var includeFiles = Request.GetBooleanQueryParameter("includeFiles");


            return _fileSystemLookupService.LookupContents((string)pathQuery.Value, includeFiles).AsResponse();
        }

        private Response GetEntityType()
        {
            var pathQuery = Request.Query.path;
            var path = (string)pathQuery.Value;

            if (_diskProvider.FileExists(path))
            {
                return new { type = "file" }.AsResponse();
            }

            //Return folder even if it doesn't exist on disk to avoid leaking anything from the UI about the underlying system
            return new { type = "folder" }.AsResponse();
        }

        private Response GetMediaFiles()
        {
            var pathQuery = Request.Query.path;
            var path = (string)pathQuery.Value;

            if (!_diskProvider.FolderExists(path))
            {
                return new string[0].AsResponse();
            }

            return _diskScanService.GetVideoFiles(path).Select(f => new {
                Path = f,
                RelativePath = path.GetRelativePath(f),
                Name = Path.GetFileName(f)
            }).AsResponse();
        }
    }
}