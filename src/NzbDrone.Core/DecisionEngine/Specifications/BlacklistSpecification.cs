using System.Linq;
using NLog;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class BlacklistSpecification : IDecisionEngineSpecification
    {
        private readonly IBlacklistService _blacklistService;
        private readonly Logger _logger;

        public BlacklistSpecification(IBlacklistService blacklistService, Logger logger)
        {
            _blacklistService = blacklistService;
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
            if (_blacklistService.Blacklisted(subject.Release.Title))
            {
                _logger.Trace("Release is blacklisted");
                return false;
            }

            return true;
        }
    }
}
