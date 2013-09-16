using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotSampleSpecification : IDecisionEngineSpecification
    {
        public string RejectionReason { get { return "Sample"; } }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.Release.Title.ToLower().Contains("sample") && subject.Release.Size < 70.Megabytes())
            {
                return false;
            }

            return true;
        }
    }
}
