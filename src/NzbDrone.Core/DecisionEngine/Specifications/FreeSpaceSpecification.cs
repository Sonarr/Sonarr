using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class FreeSpaceSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public FreeSpaceSpecification(IConfigService configService, IDiskProvider diskProvider, Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (_configService.SkipFreeSpaceCheckWhenImporting)
            {
                _logger.Debug("Skipping free space check");
                return Decision.Accept();
            }

            var size = subject.Release.Size;
            var freeSpace = _diskProvider.GetAvailableSpace(subject.Series.Path);

            if (!freeSpace.HasValue)
            {
                _logger.Debug("Unable to get available space for {0}. Skipping", subject.Series.Path);

                return Decision.Accept();
            }

            var minimumSpace = _configService.MinimumFreeSpaceWhenImporting.Megabytes();
            var remainingSpace = freeSpace.Value - size;

            if (remainingSpace <= 0)
            {
                var message = "Importing after download will exceed available disk space";

                _logger.Debug(message);
                return Decision.Reject(message);
            }

            if (remainingSpace < minimumSpace)
            {
                var message = $"Not enough free space ({minimumSpace.SizeSuffix()}) to import after download: {remainingSpace.SizeSuffix()}. (Settings: Media Management: Minimum Free Space)";

                _logger.Debug(message);
                return Decision.Reject(message);
            }

            return Decision.Accept();
        }
    }
}
