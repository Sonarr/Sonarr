using System.Collections.Generic;

namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalPrioritizationResponse
    {
        public Dictionary<string, int> Scores { get; set; }
    }
}
