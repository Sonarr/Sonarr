using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class IndexHtmlMapper : StaticResourceMapperBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly string _indexPath;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _diskProvider = diskProvider;
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