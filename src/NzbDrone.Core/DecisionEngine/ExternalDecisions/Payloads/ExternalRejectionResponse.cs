namespace NzbDrone.Core.DecisionEngine.ExternalDecisions.Payloads
{
    public class ExternalRejectionResponse
    {
        public bool Approved { get; set; } = true;
        public string Reason { get; set; }
    }
}
