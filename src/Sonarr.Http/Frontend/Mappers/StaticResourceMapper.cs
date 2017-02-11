using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class StaticResourceMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public StaticResourceMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            if (resourceUrl.StartsWith("/Content/Images/Icons/manifest") ||
                resourceUrl.StartsWith("/Content/Images/Icons/browserconfig"))
            {
                return false;
            }

            return resourceUrl.StartsWith("/Content") ||
                   resourceUrl.EndsWith(".js") ||
                   resourceUrl.EndsWith(".map") ||
                   resourceUrl.EndsWith(".css") ||
                   (resourceUrl.EndsWith(".ico") && !resourceUrl.Equals("/favicon.ico")) ||
                   resourceUrl.EndsWith(".swf") ||
                   resourceUrl.EndsWith("oauth.html");
        }
    }
}