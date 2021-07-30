using NzbDrone.Common.Exceptions;

namespace NzbDrone.Mono.Disk
{
    public class LinuxPermissionsException : NzbDroneException
    {
        public LinuxPermissionsException(string message, params object[] args)
            : base(message, args)
        {
        }

        public LinuxPermissionsException(string message)
            : base(message)
        {
        }
    }
}
