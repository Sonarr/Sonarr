using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Api.Frontend.Mappers
{
    public class MediaCoverMapper : StaticResourceMapperBase
    {
        private static readonly Regex RegexResizedImage = new Regex(@"-\d+\.jpg($|\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IAppFolderInfo _appFolderInfo;

        public MediaCoverMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.Combine(_appFolderInfo.GetAppDataPath(), path);
        }

        public override Response GetResponse(string resourceUrl)
        {
            var result =  base.GetResponse(resourceUrl);

            // Return the full sized image if someone requests a non-existing resized one.
            // TODO: This code can be removed later once everyone had the update for a while.
            if (result is NotFoundResponse)
            {
                var baseResourceUrl = RegexResizedImage.Replace(resourceUrl, ".jpg$1");
                if (baseResourceUrl != resourceUrl)
                {
                    result = base.GetResponse(baseResourceUrl);
                }
            }

            return result;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/MediaCover");
        }
    }
}