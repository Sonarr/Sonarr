using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MultiSeasonSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MultiSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedEpisodeInfo.IsMultiSeason)
            {
                _logger.Debug("Multi-season release {0} rejected. Not supported", subject.Release.Title);
                return Decision.Reject("Multi-season releases are not supported");
            }

            return Decision.Accept();
        }
    }
}
