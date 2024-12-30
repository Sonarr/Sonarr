using Workarr.Exceptions;

namespace Workarr.Disk
{
    public class NotParentException : WorkarrException
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
