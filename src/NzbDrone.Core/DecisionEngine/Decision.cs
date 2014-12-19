using System;

namespace NzbDrone.Core.DecisionEngine
{
    public class Decision
    {
        public Boolean Accepted { get; private set; }
        public String Reason { get; private set; }

        private static readonly Decision AcceptDecision = new Decision { Accepted = true };
        private Decision()
        {
        }

        public static Decision Accept()
        {
            return AcceptDecision;
        }

        public static Decision Reject(String reason, params object[] args)
        {
            return Reject(String.Format(reason, args));
        }

        public static Decision Reject(String reason)
        {
            return new Decision
            {
                Accepted = false,
                Reason = reason
            };
        }
    }
}
