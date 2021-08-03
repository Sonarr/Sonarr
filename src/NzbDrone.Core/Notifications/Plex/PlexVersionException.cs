using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexVersionException : NzbDroneException
    {
        public PlexVersionException(string message)
            : base(message)
        {
        }

        public PlexVersionException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
