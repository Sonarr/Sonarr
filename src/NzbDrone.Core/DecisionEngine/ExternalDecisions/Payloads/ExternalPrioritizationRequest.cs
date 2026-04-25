using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalPrioritizationRequest
    {
        public string DecisionType { get; set; } = nameof(ExternalDecisionType.Prioritization);
        public List<ExternalReleasePayload> Releases { get; set; }
        public ExternalSeriesPayload Series { get; set; }
        public List<ExternalEpisodePayload> Episodes { get; set; }
    }
}
