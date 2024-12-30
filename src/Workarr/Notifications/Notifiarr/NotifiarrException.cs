using Workarr.Exceptions;

namespace Workarr.Notifications.Notifiarr
{
    public class NotifiarrException : WorkarrException
    {
        public NotifiarrException(string message)
            : base(message)
        {
        }

        public NotifiarrException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
