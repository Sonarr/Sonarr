using System;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class InvalidApiKeyException : Exception
    {
        public InvalidApiKeyException()
        {
        }

        public InvalidApiKeyException(string message) : base(message)
        {
        }
    }
}
