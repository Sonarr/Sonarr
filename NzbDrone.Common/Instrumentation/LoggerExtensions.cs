using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Messaging.Tracking;

namespace NzbDrone.Common.Instrumentation
{
    public static class LoggerExtensions
    {
        public static void Progress(this Logger logger, string message)
        {
            LogProgressMessage(logger, message, ProcessState.Running);
        }

        public static void Progress(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            Progress(logger, formattedMessage);
        }

        public static void Complete(this Logger logger, string message)
        {
            LogProgressMessage(logger, message, ProcessState.Completed);
        }

        public static void Complete(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            Complete(logger, formattedMessage);
        }

        public static void Failed(this Logger logger, string message)
        {
            LogProgressMessage(logger, message, ProcessState.Failed);
        }

        public static void Failed(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            Failed(logger, formattedMessage);
        }

        private static void LogProgressMessage(Logger logger, string message, ProcessState state)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);
            logEvent.Properties.Add("Status", state);

            logger.Log(logEvent);
        }
    }
}
