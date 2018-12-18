using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Discord
{
    class DiscordExeption : NzbDroneException
    {
        public DiscordExeption(string message) : base(message)
        {
        }

        public DiscordExeption(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }
}