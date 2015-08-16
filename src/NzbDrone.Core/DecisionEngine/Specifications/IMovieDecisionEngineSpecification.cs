using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IMovieDecisionEngineSpecification : IDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria);
    }
}
