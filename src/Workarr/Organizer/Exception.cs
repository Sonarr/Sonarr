using Workarr.Exceptions;

namespace Workarr.Organizer
{
    public class NamingFormatException : WorkarrException
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
