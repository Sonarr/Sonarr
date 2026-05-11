using NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions
{
    public interface IExternalDecision : IProvider
    {
        ExternalRejectionResponse EvaluateRejection(ExternalRejectionRequest request);
        ExternalPrioritizationResponse EvaluatePrioritization(ExternalPrioritizationRequest request);
    }
}
