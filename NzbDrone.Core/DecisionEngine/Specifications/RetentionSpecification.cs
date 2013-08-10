using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RetentionSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public RetentionSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }


        public string RejectionReason
        {
            get
            {
                return "Report past retention limit.";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var age = subject.Report.Age;

            _logger.Trace("Checking if report meets retention requirements. {0}", age);
            if (_configService.Retention > 0 && age > _configService.Retention)
            {
                _logger.Trace("Report age: {0} rejected by user's retention limit", age);
                return false;
            }

            return true;
        }
    }
}
