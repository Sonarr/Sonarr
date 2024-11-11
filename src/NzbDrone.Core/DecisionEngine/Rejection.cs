namespace NzbDrone.Core.DecisionEngine
{
    public class Rejection<TRejectionReason>
    {
        public TRejectionReason Reason { get; set; }
        public string Message { get; set; }
        public RejectionType Type { get; set; }

        public Rejection(TRejectionReason reason, string message, RejectionType type = RejectionType.Permanent)
        {
            Reason = reason;
            Message = message;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Type, Message);
        }
    }
}
