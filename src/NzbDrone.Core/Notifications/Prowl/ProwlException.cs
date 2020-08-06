using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class ProwlException : NzbDroneException
    {
        public ProwlException(string message)
            : base(message)
        {
        }

        public ProwlException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
