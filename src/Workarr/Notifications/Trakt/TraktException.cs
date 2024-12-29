using Workarr.Exceptions;

namespace Workarr.Notifications.Trakt
{
    public class TraktException : WorkarrException
    {
        public TraktException(string message)
            : base(message)
        {
        }

        public TraktException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
