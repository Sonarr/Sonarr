using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserException : NzbDroneException
    {
        public MediaBrowserException(string message)
            : base(message)
        {
        }

        public MediaBrowserException(string message, params object[] args)
            : base(message, args)
        {
        }

        public MediaBrowserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
