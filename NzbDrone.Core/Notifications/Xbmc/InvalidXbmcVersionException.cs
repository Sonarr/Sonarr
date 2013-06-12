using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
