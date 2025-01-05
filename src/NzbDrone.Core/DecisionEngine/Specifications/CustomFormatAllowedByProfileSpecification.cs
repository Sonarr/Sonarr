using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CustomFormatAllowedbyProfileSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public CustomFormatAllowedbyProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var minScore = subject.Series.QualityProfile.Value.MinFormatScore;
            var score = subject.CustomFormatScore;

            if (score < minScore)
            {
                return DownloadSpecDecision.Reject(DownloadRejectionReason.CustomFormatMinimumScore, "Custom Formats {0} have score {1} below Series profile minimum {2}", subject.CustomFormats.ConcatToString(), score, minScore);
            }

            _logger.Trace("Custom Format Score of {0} [{1}] above Series profile minumum {2}", score, subject.CustomFormats.ConcatToString(), minScore);

            return DownloadSpecDecision.Accept();
        }
    }
}
