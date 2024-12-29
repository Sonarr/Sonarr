using Workarr.Exceptions;

namespace Workarr.Indexers.Exceptions
{
    public class SizeParsingException : WorkarrException
    {
        public SizeParsingException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
