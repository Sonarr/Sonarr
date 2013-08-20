using System.IO;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class StaticResourceMapper : IMapHttpRequestsToDisk
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public StaticResourceMapper(IAppFolderInfo appFolderInfo)
        {
            _appFolderInfo = appFolderInfo;
        }

        public string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.Combine(_appFolderInfo.StartUpFolder, "UI", path);
        }

        public bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content") || resourceUrl.EndsWith(".js") || resourceUrl.EndsWith(".css");
        }
    }
}