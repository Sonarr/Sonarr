using Workarr.Exceptions;

namespace Workarr.Notifications.Gotify
{
    public class GotifyException : WorkarrException
    {
        public GotifyException(string message)
            : base(message)
        {
        }

        public GotifyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public GotifyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
