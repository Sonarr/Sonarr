using Workarr.Exceptions;

namespace Workarr.Parser
{
    public class InvalidDateException : WorkarrException
    {
        public InvalidDateException(string message, params object[] args)
            : base(message, args)
        {
        }

        public InvalidDateException(string message)
            : base(message)
        {
        }
    }
}
