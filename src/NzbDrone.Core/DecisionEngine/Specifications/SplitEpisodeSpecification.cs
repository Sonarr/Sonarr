using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SplitEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedEpisodeInfo.IsSplitEpisode)
            {
                _logger.Debug("Split episode release {0} rejected. Not supported", subject.Release.Title);
                return Decision.Reject("Split episode releases are not supported");
            }

            return Decision.Accept();
        }
    }
}
