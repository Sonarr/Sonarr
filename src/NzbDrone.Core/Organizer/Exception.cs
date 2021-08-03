using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Organizer
{
    public class NamingFormatException : NzbDroneException
    {
        public NamingFormatException(string message, params object[] args)
            : base(message, args)
        {
        }

        public NamingFormatException(string message)
            : base(message)
        {
        }
    }
}
