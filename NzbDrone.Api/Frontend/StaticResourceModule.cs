using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Nancy;
using Nancy.Responses;
using NzbDrone.Api.Frontend.Mappers;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public class StaticResourceModule : NancyModule
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IEnumerable<IMapHttpRequestsToDisk> _requestMappers;
        private readonly Logger _logger;
        private readonly ICached<string> _indexCache;

        private readonly bool _caseSensitive;

        public StaticResourceModule(IDiskProvider diskProvider, ICacheManger cacheManger, IEnumerable<IMapHttpRequestsToDisk> requestMappers, Logger logger)
        {
            _diskProvider = diskProvider;
            _requestMappers = requestMappers;
            _logger = logger;

            _indexCache = cacheManger.GetCache<string>(typeof(StaticResourceModule));

            Get["/{resource*}"] = x => Index();
            Get["/"] = x => Index();

            if (!RuntimeInfo.IsProduction)
            {
                _caseSensitive = true;
            }
        }

        private Response Index()
        {
            var path = Request.Url.Path;

            if (
                string.IsNullOrWhiteSpace(path) ||
                path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase) ||
                path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }


            var mapper = _requestMappers.SingleOrDefault(m => m.CanHandle(path));


            if (mapper != null)
            {
                var filePath = mapper.Map(path);

                if (_diskProvider.FileExists(filePath, _caseSensitive))
                {
                    var response = new StreamResponse(() => File.OpenRead(filePath), MimeTypes.GetMimeType(filePath));
                    //_addCacheHeaders.ToResponse(context.Request, response);

                    return response;
                }

                _logger.Warn("File {0} not found", filePath);
            }
            else
            {
                _logger.Warn("Couldn't find handler for {0}", path);
            }

            return new NotFoundResponse();

            /* htmlResponse.Contents = stream =>
                 {
                     var lastWrite = _diskProvider.GetLastFileWrite(_indexPath);
                     var text = _indexCache.Get(lastWrite.Ticks.ToString(), GetIndexText);

                     var streamWriter = new StreamWriter(stream);

                     streamWriter.Write(text);
                     streamWriter.Flush();
                 };*/

            //htmlResponse.Headers.DisableCache();
        }


        /*        private string GetIndexText()
                {
                    var text = _diskProvider.ReadAllText(_indexPath);

                    text = text.Replace(".css", ".css?v=" + BuildInfo.Version);
                    text = text.Replace(".js", ".js?v=" + BuildInfo.Version);

                    return text;
                }*/
    }
}