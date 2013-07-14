using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.EnvironmentInfo;

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
        private readonly IAddCacheHeaders _addCacheHeaders;
        private readonly Logger _logger;

        public StaticResourceProvider(IDiskProvider diskProvider,
                                      IEnumerable<IMapHttpRequestsToDisk> requestMappers,
                                      IAddCacheHeaders addCacheHeaders,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _requestMappers = requestMappers;
            _addCacheHeaders = addCacheHeaders;
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
                    _addCacheHeaders.ToResponse(context.Request, response);

                    return response;
                }

                _logger.Warn("File {0} not found", filePath);
            }

            return null;
        }
    }
}