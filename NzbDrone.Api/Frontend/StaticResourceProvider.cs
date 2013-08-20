using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Api.Frontend.Mappers;
using NzbDrone.Common;
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

        private readonly bool _caseSensitive;

        public StaticResourceProvider(IDiskProvider diskProvider,
                                      IEnumerable<IMapHttpRequestsToDisk> requestMappers,
                                      IAddCacheHeaders addCacheHeaders,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _requestMappers = requestMappers;
            _addCacheHeaders = addCacheHeaders;
            _logger = logger;

            if (!RuntimeInfo.IsProduction)
            {
                _caseSensitive = true;
            }
        }

        public Response ProcessStaticResourceRequest(NancyContext context, string workingFolder)
        {
            var path = context.Request.Url.Path;

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (context.Request.Headers.IfModifiedSince.HasValue)
            {
                var response = new Response { ContentType = MimeTypes.GetMimeType(path), StatusCode = HttpStatusCode.NotModified };
                _addCacheHeaders.ToResponse(context.Request, response);
                return response;
            }

            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                var filePath = mapper.Map(path);

                if (_diskProvider.FileExists(filePath, _caseSensitive))
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