using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RetentionSpecification : IFetchableSpecification
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

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            _logger.Trace("Checking if report meets retention requirements. {0}", subject.Age);
            if (_configService.Retention > 0 && subject.Age > _configService.Retention)
            {
                _logger.Trace("Report age: {0} rejected by user's retention limit", subject.Age);
                return false;
            }

            return true;
        }
    }
}
