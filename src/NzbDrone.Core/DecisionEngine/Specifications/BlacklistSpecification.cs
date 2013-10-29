using System.Linq;
using NLog;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class BlacklistSpecification : IDecisionEngineSpecification
    {
        private readonly IBlacklistService _blacklistService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public BlacklistSpecification(IBlacklistService blacklistService, IConfigService configService, Logger logger)
        {
            _blacklistService = blacklistService;
            _configService = configService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Release is blacklisted";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!_configService.EnableFailedDownloadHandling)
            {
                _logger.Trace("Failed Download Handling is not enabled");
                return true;
            }

            if (_blacklistService.Blacklisted(subject.Release.Title))
            {
                _logger.Trace("{0} is blacklisted", subject.Release.Title);
                return false;
            }

            return true;
        }
    }
}
