using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Mailgun
{
    public class MailgunException : NzbDroneException
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
