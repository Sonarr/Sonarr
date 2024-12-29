using Workarr.Exceptions;

namespace Workarr.Notifications.Slack
{
    public class SlackExeption : WorkarrException
    {
        public SlackExeption(string message)
            : base(message)
        {
        }

        public SlackExeption(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
