using System;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message) : base(message)
        {
        }
    }
}
