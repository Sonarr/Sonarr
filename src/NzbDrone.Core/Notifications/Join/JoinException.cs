using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinException : NzbDroneException
    {
        public JoinException(string message)
            : base(message)
        {
        }

        public JoinException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
