using System;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDecisionEngineSpecification : IRejectWithReason
    {
        RejectionType Type { get; }
        
        Boolean IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria);
    }
}