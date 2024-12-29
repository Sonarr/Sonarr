using Workarr.Exceptions;

namespace NzbDrone.Mono.Disk
{
    public class LinuxPermissionsException : WorkarrException
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
