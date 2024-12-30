using Workarr.Exceptions;

namespace Workarr.Indexers.Exceptions
{
    public class ApiKeyException : WorkarrException
    {
        public ApiKeyException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ApiKeyException(string message)
            : base(message)
        {
        }
    }
}
