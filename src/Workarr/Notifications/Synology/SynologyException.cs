using Workarr.Exceptions;

namespace Workarr.Notifications.Synology
{
    public class SynologyException : WorkarrException
    {
        public SynologyException(string message)
            : base(message)
        {
        }

        public SynologyException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
