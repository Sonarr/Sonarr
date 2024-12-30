using System.IO;
using NLog;
using Workarr.Disk;
using Workarr.EnvironmentInfo;
using Workarr.Extensions;

namespace Sonarr.Http.Frontend.Mappers
{
    public class LogFileMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public LogFileMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = Path.GetFileName(path);

            return Path.Combine(_appFolderInfo.GetLogFolder(), path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/logfile/") && resourceUrl.EndsWith(".txt");
        }
    }
}
