using Workarr.Exceptions;

namespace Workarr.Messaging.Commands
{
    public class CommandFailedException : WorkarrException
    {
        public CommandFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public CommandFailedException(string message)
            : base(message)
        {
        }

        public CommandFailedException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }

        public CommandFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CommandFailedException(Exception innerException)
            : base("Failed", innerException)
        {
        }
    }
}
