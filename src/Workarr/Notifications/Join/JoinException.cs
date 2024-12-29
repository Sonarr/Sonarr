using Workarr.Exceptions;

namespace Workarr.Notifications.Join
{
    public class JoinException : WorkarrException
    {
        public JoinException(string message)
            : base(message)
        {
        }

        public JoinException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
