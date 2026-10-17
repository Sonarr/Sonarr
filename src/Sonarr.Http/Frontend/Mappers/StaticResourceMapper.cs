using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public class StaticResourceMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public StaticResourceMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        protected override string FolderPath => _appFolderInfo.GetUiFolder();

        protected override string MapPath(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.Combine(FolderPath, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            if (resourceUrl.StartsWith("/content/manifest") ||
                resourceUrl.StartsWith("/content/browserconfig"))
            {
                return false;
            }

            return resourceUrl.StartsWith("/content") ||
                   resourceUrl.EndsWith(".js") ||
                   resourceUrl.EndsWith(".map") ||
                   resourceUrl.EndsWith(".css") ||
                   (resourceUrl.EndsWith(".ico") && !resourceUrl.Equals("/favicon.ico")) ||
                   resourceUrl.EndsWith(".swf") ||
                   resourceUrl.EndsWith("oauth.html");
        }
    }
}
