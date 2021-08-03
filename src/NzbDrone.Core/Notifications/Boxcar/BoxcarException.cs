using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Boxcar
{
    public class BoxcarException : NzbDroneException
    {
        public BoxcarException(string message)
            : base(message)
        {
        }

        public BoxcarException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
