using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MinimumAgeSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MinimumAgeSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Temporary;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.DownloadProtocol != Indexers.DownloadProtocol.Usenet)
            {
                _logger.Debug("Not checking minimum age requirement for non-usenet report");
                return Decision.Accept();
            }

            var age = subject.Release.AgeMinutes;
            var minimumAge = _configService.MinimumAge;
            var ageRounded = Math.Round(age, 1);

            if (minimumAge == 0)
            {
                _logger.Debug("Minimum age is not set.");
                return Decision.Accept();
            }

            _logger.Debug("Checking if report meets minimum age requirements. {0}", ageRounded);

            if (age < minimumAge)
            {
                _logger.Debug("Only {0} minutes old, minimum age is {1} minutes", ageRounded, minimumAge);
                return Decision.Reject("Only {0} minutes old, minimum age is {1} minutes", ageRounded, minimumAge);
            }

            _logger.Debug("Release is {0} minutes old, greater than minimum age of {1} minutes", ageRounded, minimumAge);

            return Decision.Accept();
        }
    }
}
