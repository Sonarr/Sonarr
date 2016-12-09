using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SameEpisodesGrabSpecification : IDecisionEngineSpecification
    {
        private readonly SameEpisodesSpecification _sameEpisodesSpecification;
        private readonly Logger _logger;

        public SameEpisodesGrabSpecification(SameEpisodesSpecification sameEpisodesSpecification, Logger logger)
        {
            _sameEpisodesSpecification = sameEpisodesSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (_sameEpisodesSpecification.IsSatisfiedBy(subject.Episodes))
            {
                return Decision.Accept();
            }

            _logger.Debug("Episode file on disk contains more episodes than this release contains");
            return Decision.Reject("Episode file on disk contains more episodes than this release contains");
        }
    }
}
