using System.IO;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public class MediaCoverMapper : IMapHttpRequestsToDisk
    {
        private readonly IAppDirectoryInfo _appDirectoryInfo;

        public MediaCoverMapper(IAppDirectoryInfo appDirectoryInfo)
        {
            _appDirectoryInfo = appDirectoryInfo;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();

            return Path.Combine(_appDirectoryInfo.GetAppDataPath(), path);
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/mediacover");
        }
    }
}