using System.IO;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend
{
    public class MediaCoverMapper : IMapHttpRequestsToDisk
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public MediaCoverMapper(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar).ToLower();

            return Path.Combine(_appFolderInfo.GetAppDataPath(), path);
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/mediacover");
        }
    }
}