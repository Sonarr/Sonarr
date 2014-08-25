using System;

namespace NzbDrone.Core.DecisionEngine
{
    public class Rejection
    {
        public String Reason { get; set; }
        public RejectionType Type { get; set; }

        public Rejection(string reason, RejectionType type = RejectionType.Permanent)
        {
            Reason = reason;
            Type = type;
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", Type, Reason);
        }
    }
}
