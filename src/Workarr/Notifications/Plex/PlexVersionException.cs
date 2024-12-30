using Workarr.Exceptions;

namespace Workarr.Notifications.Plex
{
    public class PlexVersionException : WorkarrException
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
