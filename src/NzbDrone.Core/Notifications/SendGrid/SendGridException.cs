using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.SendGrid
{
    public class SendGridException : NzbDroneException
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
