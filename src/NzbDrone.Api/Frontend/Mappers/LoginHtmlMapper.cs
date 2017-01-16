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
    public class LoginHtmlMapper : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Func<ICacheBreakerProvider> _cacheBreakProviderFactory;
        private readonly string _indexPath;
        private static readonly Regex ReplaceRegex = new Regex("(?<=(?:href|src|data-main)=\").*?(?=\")", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string URL_BASE;
        private string _generatedContent;

        public LoginHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               Func<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
            _configFileProvider = configFileProvider;
            _cacheBreakProviderFactory = cacheBreakProviderFactory;
            _indexPath = Path.Combine(appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, "login.html");

            URL_BASE = configFileProvider.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return _indexPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/login");
        }

        public override Response GetResponse(string resourceUrl)
        {
            var response = base.GetResponse(resourceUrl);
            response.Headers["X-UA-Compatible"] = "IE=edge";

            return response;
        }

        protected override Stream GetContentStream(string filePath)
        {
            var text = GetLoginText();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private string GetLoginText()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var text = _diskProvider.ReadAllText(_indexPath);

            var cacheBreakProvider = _cacheBreakProviderFactory();

            text = ReplaceRegex.Replace(text, match =>
            {
                var url = cacheBreakProvider.AddCacheBreakerToPath(match.Value);
                return URL_BASE + url;
            });

            _generatedContent = text;

            return _generatedContent;
        }
    }
}
