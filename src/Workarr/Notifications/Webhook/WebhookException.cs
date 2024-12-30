using Workarr.Exceptions;

namespace Workarr.Notifications.Webhook
{
    public class WebhookException : WorkarrException
    {
        public WebhookException(string message)
            : base(message)
        {
        }

        public WebhookException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
