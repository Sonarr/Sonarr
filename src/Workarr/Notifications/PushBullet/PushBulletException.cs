using Workarr.Exceptions;

namespace Workarr.Notifications.PushBullet
{
    public class PushBulletException : WorkarrException
    {
        public PushBulletException(string message)
            : base(message)
        {
        }

        public PushBulletException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
