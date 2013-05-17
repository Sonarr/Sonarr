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

            if (path.StartsWith("/mediacover"))
            {
                var filePath = _requestMappers.Single(r => r.IHandle == RequestType.MediaCovers).Map(path);

                if (_diskProvider.FileExists(filePath))
                {
                    return new StreamResponse(() => File.OpenRead(filePath), "image/jpeg");
                }

                _logger.Warn("Couldn't find file [{0}] for [{1}]", filePath, path);
            }

            if (IsStaticResource(path))
            {
                var filePath = _requestMappers.Single(r => r.IHandle == RequestType.StaticResources).Map(path);

                if (_diskProvider.FileExists(filePath))
                {
                    return new GenericFileResponse(filePath);
                }
                
                _logger.Warn("Couldn't find file [{0}] for [{1}]", filePath, path);
            }

            return null;
        }


        private static readonly string[] Extensions = new[] { ".css", ".js", ".html", ".htm", ".jpg", ".jpeg", ".icon", ".gif", ".png", ".woff", ".ttf" };


        private bool IsStaticResource(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return Extensions.Any(path.EndsWith);
        }
    }
}