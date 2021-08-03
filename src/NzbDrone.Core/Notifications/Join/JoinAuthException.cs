using System;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinAuthException : JoinException
    {
        public JoinAuthException(string message)
            : base(message)
        {
        }

        public JoinAuthException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
