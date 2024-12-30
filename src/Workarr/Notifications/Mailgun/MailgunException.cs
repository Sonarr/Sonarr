using Workarr.Exceptions;

namespace Workarr.Notifications.Mailgun
{
    public class MailgunException : WorkarrException
    {
        public MailgunException(string message)
            : base(message)
        {
        }

        public MailgunException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
