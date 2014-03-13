using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotSampleSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        public string RejectionReason { get { return "Sample"; } }

        public NotSampleSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.Title.ToLower().Contains("sample") && subject.Release.Size < 70.Megabytes())
            {
                _logger.Debug("Sample release, rejecting.");
                return false;
            }

            return true;
        }
    }
}
