using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletException : NzbDroneException
    {
        public PushBulletException(string message)
            : base(message)
        {
        }

        public PushBulletException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
