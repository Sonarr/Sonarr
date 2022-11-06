using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class NotifiarrException : NzbDroneException
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
