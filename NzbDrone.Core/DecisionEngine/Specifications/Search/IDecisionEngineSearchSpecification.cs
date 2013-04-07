using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public interface IDecisionEngineSearchSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(IndexerParseResult indexerParseResult, SearchDefinitionBase searchDefinitionBase);
    }
}