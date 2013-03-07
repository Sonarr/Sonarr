using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IFetchableSpecification
    {
        string RejectionReason { get; }
        bool IsSatisfiedBy(EpisodeParseResult subject);
    }
}