using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Apprise
{
    public class AppriseException : NzbDroneException
    {
        public AppriseException(string message)
            : base(message)
        {
        }

        public AppriseException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
