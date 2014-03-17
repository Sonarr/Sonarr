using System;
using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class IndexHtmlMapper : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly string _indexPath;
        private static readonly Regex ReplaceRegex = new Regex("(?<=(?:href|src|data-main)=\").*?(?=\")", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static String API_KEY;
        private static String URL_BASE;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _indexPath = Path.Combine(appFolderInfo.StartUpFolder, "UI", "index.html");

            API_KEY = configFileProvider.ApiKey;
            URL_BASE = configFileProvider.UrlBase;
        }

        protected override string Map(string resourceUrl)
        {
            return _indexPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return !resourceUrl.Contains(".");
        }

        public override Response GetResponse(string resourceUrl)
        {
            var response = base.GetResponse(resourceUrl);
            response.Headers["X-UA-Compatible"] = "IE=edge";

            return response;
        }

        protected override Stream GetContentStream(string filePath)
        {
            return StringToStream(GetIndexText());
        }

        private string GetIndexText()
        {
            var text = _diskProvider.ReadAllText(_indexPath);

            text = ReplaceRegex.Replace(text, match => URL_BASE + match.Value);

            text = text.Replace(".css", ".css?v=" + BuildInfo.Version);
            text = text.Replace(".js", ".js?v=" + BuildInfo.Version);
            text = text.Replace("API_ROOT", URL_BASE + "/api");
            text = text.Replace("API_KEY", API_KEY);
            text = text.Replace("APP_VERSION", BuildInfo.Version.ToString());

            return text;
        }
    }
}