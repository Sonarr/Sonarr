using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDecisionEngineSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria);
    }
}