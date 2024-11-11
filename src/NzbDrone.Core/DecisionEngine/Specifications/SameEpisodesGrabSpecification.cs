using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SameEpisodesGrabSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly SameEpisodesSpecification _sameEpisodesSpecification;
        private readonly Logger _logger;

        public SameEpisodesGrabSpecification(SameEpisodesSpecification sameEpisodesSpecification, Logger logger)
        {
            _sameEpisodesSpecification = sameEpisodesSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (_sameEpisodesSpecification.IsSatisfiedBy(subject.Episodes))
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Episode file on disk contains more episodes than this release contains");
            return DownloadSpecDecision.Reject(DownloadRejectionReason.ExistingFileHasMoreEpisodes, "Episode file on disk contains more episodes than this release contains");
        }
    }
}
