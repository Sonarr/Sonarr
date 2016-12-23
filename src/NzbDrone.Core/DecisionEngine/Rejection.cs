namespace NzbDrone.Core.DecisionEngine
{
    public class Rejection
    {
        public string Reason { get; set; }
        public RejectionType Type { get; set; }

        public Rejection(string reason, RejectionType type = RejectionType.Permanent)
        {
            Reason = reason;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Type, Reason);
        }
    }
}
