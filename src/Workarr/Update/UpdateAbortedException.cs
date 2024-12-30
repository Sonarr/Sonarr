using Workarr.Exceptions;

namespace Workarr.Update
{
    public class UpdateFailedException : WorkarrException
    {
        public UpdateFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateFailedException(string message)
            : base(message)
        {
        }
    }
}
