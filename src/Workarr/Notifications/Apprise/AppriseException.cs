using Workarr.Exceptions;

namespace Workarr.Notifications.Apprise
{
    public class AppriseException : WorkarrException
    {
        public AppriseException(string message)
            : base(message)
        {
        }

        public AppriseException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
