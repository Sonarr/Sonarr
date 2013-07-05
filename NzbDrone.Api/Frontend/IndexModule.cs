using System;
using System.IO;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Api.Extensions;

namespace NzbDrone.Api.Frontend
{
    public class IndexModule : NancyModule
    {
        private readonly IDiskProvider _diskProvider;
        private readonly ICached<string> _indexCache;

        private readonly string _indexPath;

        public IndexModule(IDiskProvider diskProvider, ICacheManger cacheManger, IAppFolderInfo appFolder)
        {
            _diskProvider = diskProvider;

            _indexPath = Path.Combine(appFolder.StartUpFolder, "UI", "index.html");

            _indexCache = cacheManger.GetCache<string>(typeof(IndexModule));
            //Serve anything that doesn't have an extension
            Get[@"/(.*)"] = x => Index();
        }

        private object Index()
        {
            if (
                Request.Path.Contains(".")
                || Request.Path.StartsWith("/static", StringComparison.CurrentCultureIgnoreCase)
                || Request.Path.StartsWith("/api", StringComparison.CurrentCultureIgnoreCase)
                || Request.Path.StartsWith("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return new NotFoundResponse();
            }


            var htmlResponse = new HtmlResponse();

            htmlResponse.Contents = stream =>
                {
                    var lastWrite = _diskProvider.GetLastFileWrite(_indexPath);
                    var text = _indexCache.Get(lastWrite.Ticks.ToString(), GetIndexText);

                    var streamWriter = new StreamWriter(stream);

                    streamWriter.Write(text);
                    streamWriter.Flush();
                };


            htmlResponse.Headers.DisableCache();

            return htmlResponse;
        }


        private string GetIndexText()
        {
            var text = _diskProvider.ReadAllText(_indexPath);

            text = text.Replace(".css", ".css?v=" + BuildInfo.Version);
            text = text.Replace(".js", ".js?v=" + BuildInfo.Version);

            return text;
        }
    }
}