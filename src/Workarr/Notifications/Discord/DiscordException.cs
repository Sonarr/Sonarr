using Workarr.Exceptions;

namespace Workarr.Notifications.Discord
{
    public class DiscordException : WorkarrException
    {
        public DiscordException(string message)
            : base(message)
        {
        }

        public DiscordException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
