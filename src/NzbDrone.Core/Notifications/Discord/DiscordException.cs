using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Discord
{
    public class DiscordException : NzbDroneException
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
