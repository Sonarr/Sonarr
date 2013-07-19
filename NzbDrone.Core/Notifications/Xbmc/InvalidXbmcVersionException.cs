using System;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class InvalidXbmcVersionException : Exception
    {
        public InvalidXbmcVersionException()
        {
        }

        public InvalidXbmcVersionException(string message) : base(message)
        {
        }
    }
}
