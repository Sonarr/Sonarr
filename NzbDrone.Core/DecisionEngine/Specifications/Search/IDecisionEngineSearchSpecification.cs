using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public interface IDecisionEngineSearchSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase);
    }
}