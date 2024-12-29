using Workarr.Exceptions;

namespace Workarr.Notifications.Prowl
{
    public class ProwlException : WorkarrException
    {
        public ProwlException(string message)
            : base(message)
        {
        }

        public ProwlException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
