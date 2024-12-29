using NLog;
using Workarr.Extensions;
using Workarr.IndexerSearch.Definitions;
using Workarr.Parser.Model;

namespace Workarr.DecisionEngine.Specifications
{
    public class NotSampleSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public NotSampleSpecification(Logger logger)
        {
            _logger = logger;
        }

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.Title.ToLower().Contains("sample") && subject.Release.Size < 70.Megabytes())
            {
                _logger.Debug("Sample release, rejecting.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.Sample, "Sample");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
