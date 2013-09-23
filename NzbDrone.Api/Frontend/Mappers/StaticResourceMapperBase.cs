using System.IO;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public abstract class StaticResourceMapperBase : IMapHttpRequestsToDisk
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;
        private readonly bool _caseSensitive;

        private static readonly NotFoundResponse NotFoundResponse = new NotFoundResponse();

        protected StaticResourceMapperBase(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            if (!RuntimeInfo.IsProduction)
            {
                _caseSensitive = true;
            }


        }

        protected abstract string Map(string resourceUrl);


        public abstract bool CanHandle(string resourceUrl);

        public virtual Response GetResponse(string resourceUrl)
        {
            var filePath = Map(resourceUrl);

            if (_diskProvider.FileExists(filePath, _caseSensitive))
            {
                var response = new StreamResponse(() => GetContentStream(filePath), MimeTypes.GetMimeType(filePath));
                return response;
            }

            _logger.Warn("File {0} not found", filePath);

            return NotFoundResponse;
        }

        protected virtual Stream GetContentStream(string filePath)
        {
            return File.OpenRead(filePath);
        }

        protected static Stream StringToStream(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}