using Workarr.Exceptions;

namespace Workarr.Notifications.Plex
{
    public class PlexException : WorkarrException
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
