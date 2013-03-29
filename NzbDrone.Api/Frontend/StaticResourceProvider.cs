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
        private readonly DiskProvider _diskProvider;
        private readonly IMapHttpRequestsToDisk _requestMapper;
        private readonly Logger _logger;

        public StaticResourceProvider(DiskProvider diskProvider, IMapHttpRequestsToDisk requestMapper, Logger logger)
        {
            _diskProvider = diskProvider;
            _requestMapper = requestMapper;
            _logger = logger;
        }

        public Response ProcessStaticResourceRequest(NancyContext context, string workingFolder)
        {
            var path = context.Request.Url.Path.ToLower();

            if (IsStaticResource(path))
            {
                var filePath = _requestMapper.Map(path);

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