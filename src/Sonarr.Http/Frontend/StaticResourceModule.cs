using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NLog;
using Sonarr.Http.Frontend.Mappers;

namespace Sonarr.Http.Frontend
{
    public class StaticResourceModule : NancyModule
    {
        private readonly IEnumerable<IMapHttpRequestsToDisk> _requestMappers;
        private readonly Logger _logger;


        public StaticResourceModule(IEnumerable<IMapHttpRequestsToDisk> requestMappers, Logger logger)
        {
            _requestMappers = requestMappers;
            _logger = logger;

            Get["/{resource*}"] = x => Index();
            Get["/"] = x => Index();
        }

        private Response Index()
        {
            var path = Request.Url.Path;

            if (
                string.IsNullOrWhiteSpace(path) ||
                path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase) ||
                path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return new NotFoundResponse();
            }

            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));

            if (mapper != null)
            {
                return mapper.GetResponse(path);
            }

            _logger.Warn("Couldn't find handler for {0}", path);

            return new NotFoundResponse();
        }
    }
}
