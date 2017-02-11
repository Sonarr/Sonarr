using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public class MediaCoverMapper : StaticResourceMapperBase
    {
        private static readonly Regex RegexResizedImage = new Regex(@"-\d+\.jpg($|\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public MediaCoverMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            var resourcePath = Path.Combine(_appFolderInfo.GetAppDataPath(), path);

            if (!_diskProvider.FileExists(resourcePath) || _diskProvider.GetFileSize(resourcePath) == 0)
            {
                var baseResourcePath = RegexResizedImage.Replace(resourcePath, ".jpg$1");
                if (baseResourcePath != resourcePath)
                {
                    return baseResourcePath;
                }
            }

            return resourcePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/MediaCover", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
