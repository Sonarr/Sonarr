using System;

namespace NzbDrone.Core.DecisionEngine
{
    public class Decision
    {
        public Boolean Accepted { get; private set; }
        public String Reason { get; private set; }

        public static Decision Accept()
        {
            return new Decision
            {
                Accepted = true
            };
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
