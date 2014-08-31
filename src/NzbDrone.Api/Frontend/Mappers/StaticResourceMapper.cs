using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class StaticResourceMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public StaticResourceMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.Combine(_appFolderInfo.StartUpFolder, "UI", path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content") ||
                   resourceUrl.EndsWith(".js") ||
                   resourceUrl.EndsWith(".css") ||
                   (resourceUrl.EndsWith(".ico") && !resourceUrl.Equals("/favicon.ico")) ||
                   resourceUrl.EndsWith(".swf");
        }
    }
}