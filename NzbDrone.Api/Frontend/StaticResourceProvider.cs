using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.Frontend
{
    public interface IProcessStaticResource
    {
        Response ProcessStaticResourceRequest(NancyContext context, string workingFolder);
    }

    public class StaticResourceProvider : IProcessStaticResource
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IEnumerable<IMapHttpRequestsToDisk> _requestMappers;
        private readonly Logger _logger;

        public StaticResourceProvider(IDiskProvider diskProvider, IEnumerable<IMapHttpRequestsToDisk> requestMappers, Logger logger)
        {
            _diskProvider = diskProvider;
            _requestMappers = requestMappers;
            _logger = logger;
        }

        public Response ProcessStaticResourceRequest(NancyContext context, string workingFolder)
        {
            var path = context.Request.Url.Path.ToLower();

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                var filePath = mapper.Map(path);

                if (_diskProvider.FileExists(filePath))
                {
                    var response = new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
                    response.Headers.EnableCache();

                    return response;
                }

                _logger.Warn("File {0} not found", filePath);
            }

            return null;
        }
    }
}