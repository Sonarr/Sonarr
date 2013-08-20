using System.IO;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class IndexHtmlMapper : IMapHttpRequestsToDisk
    {
        private readonly string _indexPath;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo)
        {
            _indexPath = Path.Combine(appFolderInfo.StartUpFolder, "UI", "index.html");
        }

        public string Map(string resourceUrl)
        {
            return _indexPath;
        }

        public bool CanHandle(string resourceUrl)
        {
            return !resourceUrl.Contains(".");
        }
    }
}