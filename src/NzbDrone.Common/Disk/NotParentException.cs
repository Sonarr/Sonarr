using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Disk
{
    public class NotParentException : NzbDroneException
    {
        public NotParentException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NotParentException(string message)
            : base(message)
        {
        }
    }
}
