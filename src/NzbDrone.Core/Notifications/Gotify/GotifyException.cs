using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class GotifyException : NzbDroneException
    {
        public GotifyException(string message)
            : base(message)
        {
        }

        public GotifyException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
