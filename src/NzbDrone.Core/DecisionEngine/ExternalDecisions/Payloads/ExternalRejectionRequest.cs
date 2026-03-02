using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalRejectionRequest
    {
        public string DecisionType { get; set; } = nameof(ExternalDecisionType.Rejection);
        public ExternalReleasePayload Release { get; set; }
        public ExternalSeriesPayload Series { get; set; }
        public List<ExternalEpisodePayload> Episodes { get; set; }
        public List<ExternalExistingFilePayload> ExistingFiles { get; set; }
    }
}
