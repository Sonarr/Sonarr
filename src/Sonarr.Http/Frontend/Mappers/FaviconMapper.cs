using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class FaviconMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public FaviconMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        protected override string FolderPath => Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder);

        protected override string MapPath(string resourceUrl)
        {
            var fileName = "favicon.ico";

            if (BuildInfo.IsDebug)
            {
                fileName = "favicon-debug.ico";
            }

            var path = Path.Combine("Content", "Images", "Icons", fileName);

            return Path.Combine(FolderPath, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.Equals("/favicon.ico");
        }
    }
}
