using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Discordnotifier
{
    public class DiscordnotifierException : NzbDroneException
    {
        public DiscordnotifierException(string message) : base(message)
        {
        }

        public DiscordnotifierException(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }
}
