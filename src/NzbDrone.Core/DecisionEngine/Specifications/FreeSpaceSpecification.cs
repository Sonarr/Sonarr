using System;
using System.IO;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class FreeSpaceSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private readonly ICached<long?> _cache;

        public FreeSpaceSpecification(IConfigService configService, IDiskProvider diskProvider, ICacheManager cacheManager, Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _logger = logger;

            _cache = cacheManager.GetCache<long?>(GetType());
        }

        public SpecificationPriority Priority => SpecificationPriority.Disk;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (_configService.SkipFreeSpaceCheckWhenGrabbing)
            {
                _logger.Debug("Skipping free space check");
                return DownloadSpecDecision.Accept();
            }

            var size = subject.Release.Size;
            var path = subject.Series.Path;
            var freeSpace = _cache.Get(path, () => GetFreeSpaceForPath(path), TimeSpan.FromSeconds(5));

            if (!freeSpace.HasValue)
            {
                _logger.Debug("Unable to get available space for {0}. Skipping", path);

                return DownloadSpecDecision.Accept();
            }

            var minimumSpace = _configService.MinimumFreeSpaceWhenImporting.Megabytes();
            var remainingSpace = freeSpace.Value - size;

            if (remainingSpace <= 0)
            {
                var message = "Importing after download will exceed available disk space";

                _logger.Debug(message);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumFreeSpace, message);
            }

            if (remainingSpace < minimumSpace)
            {
                var message = $"Not enough free space ({minimumSpace.SizeSuffix()}) to import after download: {remainingSpace.SizeSuffix()}. (Settings: Media Management: Minimum Free Space)";

                _logger.Debug(message);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumFreeSpace, message);
            }

            return DownloadSpecDecision.Accept();
        }

        private long? GetFreeSpaceForPath(string path)
        {
            try
            {
                return _diskProvider.GetAvailableSpace(path);
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore so it'll be skipped in the following checks
            }

            return null;
        }
    }
}
