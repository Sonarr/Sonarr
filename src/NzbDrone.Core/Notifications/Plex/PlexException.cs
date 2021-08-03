using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexException : NzbDroneException
    {
        public PlexException(string message)
            : base(message)
        {
        }

        public PlexException(string message, params object[] args)
            : base(message, args)
        {
        }

        public PlexException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
