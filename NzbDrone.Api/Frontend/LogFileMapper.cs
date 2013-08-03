using System.IO;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public class LogFileMapper : IMapHttpRequestsToDisk
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public LogFileMapper(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = Path.GetFileName(path);

            return Path.Combine(_appFolderInfo.GetLogFolder(), path);
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/log") && resourceUrl.EndsWith(".txt");
        }
    }
}