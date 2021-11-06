using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace Sonarr.Http.Frontend.Mappers
{
    public abstract class StaticResourceMapperBase : IMapHttpRequestsToDisk
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        private readonly StringComparison _caseSensitive;
        private readonly IContentTypeProvider _mimeTypeProvider;

        protected StaticResourceMapperBase(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            _mimeTypeProvider = new FileExtensionContentTypeProvider();
            _caseSensitive = RuntimeInfo.IsProduction ? DiskProviderBase.PathStringComparison : StringComparison.OrdinalIgnoreCase;
        }

        public abstract string Map(string resourceUrl);

        public abstract bool CanHandle(string resourceUrl);

        public IActionResult GetResponse(string resourceUrl)
        {
            var filePath = Map(resourceUrl);

            if (_diskProvider.FileExists(filePath, _caseSensitive))
            {
                if (!_mimeTypeProvider.TryGetContentType(filePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                return new FileStreamResult(GetContentStream(filePath), contentType);
            }

            _logger.Warn("File {0} not found", filePath);

            return null;
        }

        protected virtual Stream GetContentStream(string filePath)
        {
            return File.OpenRead(filePath);
        }
    }
}
