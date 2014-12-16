using System;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Disk;

namespace NzbDrone.Api.FileSystem
{
    public class FileSystemModule : NzbDroneApiModule
    {
        private readonly IFileSystemLookupService _fileSystemLookupService;

        public FileSystemModule(IFileSystemLookupService fileSystemLookupService)
            : base("/filesystem")
        {
            _fileSystemLookupService = fileSystemLookupService;
            Get["/"] = x => GetContents();
        }

        private Response GetContents()
        {
            var pathQuery = Request.Query.path;
            var includeFilesQuery = Request.Query.includeFiles;
            bool includeFiles = false;

            if (includeFilesQuery.HasValue)
            {
                includeFiles = Convert.ToBoolean(includeFilesQuery.Value);
            }

            return _fileSystemLookupService.LookupContents((string)pathQuery.Value, includeFiles).AsResponse();
        }
    }
}