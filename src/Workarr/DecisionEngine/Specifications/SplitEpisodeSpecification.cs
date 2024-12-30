using NLog;
using Workarr.IndexerSearch.Definitions;
using Workarr.Parser.Model;

namespace Workarr.DecisionEngine.Specifications
{
    public class SplitEpisodeSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SplitEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedEpisodeInfo.IsSplitEpisode)
            {
                _logger.Debug("Split episode release {0} rejected. Not supported", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.SplitEpisode, "Split episode releases are not supported");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
