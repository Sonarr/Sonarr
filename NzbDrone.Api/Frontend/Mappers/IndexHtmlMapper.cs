using System;
using System.IO;
using Nancy;
using Nancy.Responses;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class IndexHtmlMapper : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly string _indexPath;

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
            string content;
            var response = base.GetResponse(resourceUrl);
            var stream = new MemoryStream();
            
            response.Contents.Invoke(stream);
            stream.Position = 0;

            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            content = content.Replace("API_KEY", _configFileProvider.ApiKey);

            response = new StreamResponse(() => StringToStream(content), response.ContentType);
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

            text = text.Replace(".css", ".css?v=" + BuildInfo.Version);
            text = text.Replace(".js", ".js?v=" + BuildInfo.Version);

            return text;
        }
    }
}