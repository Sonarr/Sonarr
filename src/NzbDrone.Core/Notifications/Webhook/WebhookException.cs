using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookException : NzbDroneException
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
