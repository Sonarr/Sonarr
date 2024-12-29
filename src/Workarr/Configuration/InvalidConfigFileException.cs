using Workarr.Exceptions;

namespace Workarr.Configuration
{
    public class InvalidConfigFileException : WorkarrException
    {
        public InvalidConfigFileException(string message)
            : base(message)
        {
        }

        public InvalidConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
