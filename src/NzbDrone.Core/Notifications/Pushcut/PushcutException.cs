using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutException : NzbDroneException
    {
        public PushcutException(string message, params object[] args)
            : base(message, args)
        {
        }

        public PushcutException(string message)
            : base(message)
        {
        }

        public PushcutException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public PushcutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
