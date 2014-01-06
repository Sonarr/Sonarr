using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class IndexHtmlMapper : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly string _indexPath;
        private static readonly Regex ReplaceRegex = new Regex("(?<=(?:href|src|data-main)=\").*?(?=\")", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _configFileProvider = configFileProvider;
            _indexPath = Path.Combine(appFolderInfo.StartUpFolder, "UI", "index.html");
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

            text = ReplaceRegex.Replace(text, match => _configFileProvider.UrlBase + match.Value);

            text = text.Replace(".css", ".css?v=" + BuildInfo.Version);
            text = text.Replace(".js", ".js?v=" + BuildInfo.Version);
            text = text.Replace("API_ROOT", _configFileProvider.UrlBase + "/api");
            text = text.Replace("API_KEY", _configFileProvider.ApiKey);
            text = text.Replace("APP_VERSION", BuildInfo.Version.ToString());

            return text;
        }
    }
}