using NLog;
using Workarr.Serializer.Newtonsoft.Json;
using Workarr.TPL;

namespace Workarr.Instrumentation.Instrumentation
{
    public static class LogEventExtensions
    {
        public static string GetHash(this LogEventInfo logEvent)
        {
            var stackString = logEvent.StackTrace.ToJson();
            var hashSeed = string.Concat(logEvent.LoggerName, logEvent.Exception.GetType().ToString(), stackString, logEvent.Level);
            return HashUtil.CalculateCrc(hashSeed);
        }

        public static string GetFormattedMessage(this LogEventInfo logEvent)
        {
            var message = logEvent.FormattedMessage;

            if (logEvent.Exception != null)
            {
                if (logEvent.Exception != null)
                {
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = logEvent.Exception.Message;
                    }
                    else
                    {
                        message += ": " + logEvent.Exception.Message;
                    }
                }
            }

            return message;
        }
    }
}
