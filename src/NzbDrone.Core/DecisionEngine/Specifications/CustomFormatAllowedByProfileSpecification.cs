using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CustomFormatAllowedbyProfileSpecification : IDownloadDecisionEngineSpecification
    {
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

            return DownloadSpecDecision.Accept();
        }
    }
}
