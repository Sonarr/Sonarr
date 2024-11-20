﻿using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MaximumSizeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MaximumSizeSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var size = subject.Release.Size;
            var maximumSize = _configService.MaximumSize.Megabytes();

            if (maximumSize == 0)
            {
                _logger.Debug("Maximum size is not set.");
                return DownloadSpecDecision.Accept();
            }

            if (size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check.");
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Checking if release meets maximum size requirements. {0}", size.SizeSuffix());

            if (size > maximumSize)
            {
                var message = $"{size.SizeSuffix()} is too big, maximum size is {maximumSize.SizeSuffix()} (Settings->Indexers->Maximum Size)";

                _logger.Debug(message);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MaximumSizeExceeded, message);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
