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
        public static void Complete(this Logger logger, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);
            logEvent.Properties.Add("Status", ProcessState.Completed);

            logger.Log(logEvent);
        }

        public static void Complete(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            Complete(logger, formattedMessage);
        }

        public static void Failed(this Logger logger, string message)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);
            logEvent.Properties.Add("Status", ProcessState.Failed);

            logger.Log(logEvent);
        }

        public static void Failed(this Logger logger, string message, params object[] args)
        {
            var formattedMessage = String.Format(message, args);
            Failed(logger, formattedMessage);
        }
    }
}
