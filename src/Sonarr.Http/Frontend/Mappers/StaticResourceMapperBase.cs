using System;
using System.IO;
using System.Threading.Tasks;
using Nancy;
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

        public async virtual Task<Response> GetResponse(string resourceUrl)
        {
            var filePath = Map(resourceUrl);

            if (_diskProvider.FileExists(filePath, _caseSensitive))
            {
                var data = await GetContent(filePath).ConfigureAwait(false);

                return new ByteArrayResponse(data, MimeTypes.GetMimeType(filePath));
            }

            _logger.Warn("File {0} not found", filePath);

            return NotFoundResponse;
        }

        protected async virtual Task<byte[]> GetContent(string filePath)
        {
            using (var output = new MemoryStream())
            {
                using (var file = File.OpenRead(filePath))
                {
                    await file.CopyToAsync(output).ConfigureAwait(false);
                }

                return output.ToArray();
            }
        }
    }
}
