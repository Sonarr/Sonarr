using System;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinInvalidDeviceException : JoinException
    {
        public JoinInvalidDeviceException(string message)
            : base(message)
        {
        }

        public JoinInvalidDeviceException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
