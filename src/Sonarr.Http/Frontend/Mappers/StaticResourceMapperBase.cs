using System;
using System.IO;
using Nancy;
using Nancy.Responses;
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

        private static readonly NotFoundResponse NotFoundResponse = new NotFoundResponse();

        protected StaticResourceMapperBase(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            _caseSensitive = RuntimeInfo.IsProduction ? DiskProviderBase.PathStringComparison : StringComparison.OrdinalIgnoreCase;
        }

        public abstract string Map(string resourceUrl);

        public abstract bool CanHandle(string resourceUrl);

        public virtual Response GetResponse(string resourceUrl)
        {
            var filePath = Map(resourceUrl);

            if (_diskProvider.FileExists(filePath, _caseSensitive))
            {
                var response = new StreamResponse(() => GetContentStream(filePath), MimeTypes.GetMimeType(filePath));
                return new MaterialisingResponse(response);
            }

            _logger.Warn("File {0} not found", filePath);

            return NotFoundResponse;
        }

        protected virtual Stream GetContentStream(string filePath)
        {
            return File.OpenRead(filePath);
        }
    }
}
