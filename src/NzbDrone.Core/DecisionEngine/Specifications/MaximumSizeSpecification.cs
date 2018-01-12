using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Configuration;
using NLog;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MaximumSizeSpecification : IDecisionEngineSpecification
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

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var size = subject.Release.Size;
            var maximumSize = _configService.MaximumSize;

            if (maximumSize == 0)
            {
                _logger.Debug("Maximum size is not set.");
                return Decision.Accept();
            }

            _logger.Debug("Checking if report meets maximum size requirements. {0}", size);

            if (size > maximumSize)
            {
                var message = $"{size} too big, maximumn size is {maximumSize} MB";

                _logger.Debug(message);
                return Decision.Reject(message);
            }

            return Decision.Accept();
        }
    }
}
