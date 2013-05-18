using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;

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

            foreach (var requestMapper in _requestMappers)
            {
                if (requestMapper.CanHandle(path))
                {
                    var filePath = requestMapper.Map(path);

                    if (_diskProvider.FileExists(filePath))
                    {
                        return new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
                    }
                }
            }

            _logger.Warn("Couldn't find a matching file for: {0}", path);
            return null;
        }
    }
}