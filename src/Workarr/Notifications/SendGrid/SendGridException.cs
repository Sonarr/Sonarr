using Workarr.Exceptions;

namespace Workarr.Notifications.SendGrid
{
    public class SendGridException : WorkarrException
    {
        public SendGridException(string message)
            : base(message)
        {
        }

        public SendGridException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
