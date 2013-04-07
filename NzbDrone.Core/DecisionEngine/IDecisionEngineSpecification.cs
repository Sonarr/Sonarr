using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDecisionEngineSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(EpisodeParseResult subject);
    }
}