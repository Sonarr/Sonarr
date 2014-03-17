using System;
using System.Text.RegularExpressions;
using NLog;

namespace NzbDrone.Core.Instrumentation.Extensions
{
    public static class LoggerCleansedExtensions
    {
        private static readonly Regex CleansingRegex = new Regex(@"(?<=apikey=)(\w+?)(?=\W|$|_)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void CleansedInfo(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            LogCleansedMessage(logger, LogLevel.Info, formattedMessage);
        }

        public static void CleansedDebug(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            LogCleansedMessage(logger, LogLevel.Debug, formattedMessage);
        }

        public static void CleansedTrace(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            LogCleansedMessage(logger, LogLevel.Trace, formattedMessage);
        }

        private static void LogCleansedMessage(Logger logger, LogLevel level, string message)
        {
            message = Cleanse(message);

            var logEvent = new LogEventInfo(level, logger.Name, message);

            logger.Log(logEvent);
        }

        private static string Cleanse(string message)
        {
            //TODO: password=

            return CleansingRegex.Replace(message, "<removed>");
        }
    }
}
