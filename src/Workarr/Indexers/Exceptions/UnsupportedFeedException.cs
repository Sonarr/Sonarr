using Workarr.Exceptions;

namespace Workarr.Indexers.Exceptions
{
    public class UnsupportedFeedException : WorkarrException
    {
        public UnsupportedFeedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UnsupportedFeedException(string message)
            : base(message)
        {
        }
    }
}
