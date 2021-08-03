using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Synology
{
    public class SynologyException : NzbDroneException
    {
        public SynologyException(string message)
            : base(message)
        {
        }

        public SynologyException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
