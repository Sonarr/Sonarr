using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface ISeriesDecisionEngineSpecification : IDecisionEngineSpecification
    {
        Decision IsSatisfiedBy(RemoteEpisode subject, SeriesSearchCriteriaBase searchCriteria);
    }
}
