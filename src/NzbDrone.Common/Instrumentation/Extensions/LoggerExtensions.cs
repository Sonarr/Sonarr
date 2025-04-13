using NLog;

namespace NzbDrone.Common.Instrumentation.Extensions
{
    public static class LoggerExtensions
    {
        [MessageTemplateFormatMethod("message")]
        public static void ProgressInfo(this Logger logger, string message, params object[] args)
        {
            LogProgressMessage(logger, LogLevel.Info, message, args);
        }

        [MessageTemplateFormatMethod("message")]
        public static void ProgressDebug(this Logger logger, string message, params object[] args)
        {
            LogProgressMessage(logger, LogLevel.Debug, message, args);
        }

        [MessageTemplateFormatMethod("message")]
        public static void ProgressTrace(this Logger logger, string message, params object[] args)
        {
            LogProgressMessage(logger, LogLevel.Trace, message, args);
        }

        private static void LogProgressMessage(Logger logger, LogLevel level, string message, object[] parameters)
        {
            var logEvent = new LogEventInfo(level, logger.Name, null, message, parameters);
            logEvent.Properties.Add("Status", "");

            logger.Log(logEvent);
        }
    }
}
