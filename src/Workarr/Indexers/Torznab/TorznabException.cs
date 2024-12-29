using Workarr.Exceptions;

namespace Workarr.Indexers.Torznab
{
    public class TorznabException : WorkarrException
    {
        public TorznabException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TorznabException(string message)
            : base(message)
        {
        }
    }
}
