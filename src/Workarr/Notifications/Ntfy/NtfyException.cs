using Workarr.Exceptions;

namespace Workarr.Notifications.Ntfy
{
    public class NtfyException : WorkarrException
    {
        public NtfyException(string message)
            : base(message)
        {
        }

        public NtfyException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
