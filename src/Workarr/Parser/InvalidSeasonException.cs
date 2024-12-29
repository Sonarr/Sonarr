using Workarr.Exceptions;

namespace Workarr.Parser
{
    public class InvalidSeasonException : WorkarrException
    {
        public InvalidSeasonException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidSeasonException(string message)
            : base(message)
        {
        }
    }
}
