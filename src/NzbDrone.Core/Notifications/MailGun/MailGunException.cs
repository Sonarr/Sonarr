using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.MailGun
{
    public class MailGunException : NzbDroneException
    {
        public MailGunException(string message) : base (message) { }
        
        public MailGunException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args) { }
    }
}
