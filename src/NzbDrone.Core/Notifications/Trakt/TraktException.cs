using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Trakt
{
    public class TraktException : NzbDroneException
    {
        public TraktException(string message)
            : base(message)
        {
        }

        public TraktException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
