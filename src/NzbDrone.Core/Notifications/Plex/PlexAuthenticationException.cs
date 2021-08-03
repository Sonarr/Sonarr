namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexAuthenticationException : PlexException
    {
        public PlexAuthenticationException(string message)
            : base(message)
        {
        }

        public PlexAuthenticationException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
