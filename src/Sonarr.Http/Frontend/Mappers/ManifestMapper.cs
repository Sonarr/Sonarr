using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend.Mappers
{
    public class ManifestMapper : UrlBaseReplacementResourceMapperBase
    {
        public ManifestMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, configFileProvider, logger)
        {
            FilePath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "Content", "manifest.json");
        }

        public override string Map(string resourceUrl)
        {
            return FilePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content/manifest");
        }
    }
}
