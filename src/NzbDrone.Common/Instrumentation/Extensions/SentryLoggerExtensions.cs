using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Fluent;

namespace NzbDrone.Common.Instrumentation.Extensions
{
    public static class SentryLoggerExtensions
    {
        public static readonly Logger SentryLogger = LogManager.GetLogger("Sentry");

        public static LogBuilder SentryFingerprint(this LogBuilder logBuilder, params string[] fingerprint)
        {
            return logBuilder.Property("Sentry", fingerprint);
        }

        public static LogBuilder WriteSentryDebug(this LogBuilder logBuilder, params string[] fingerprint)
        {
            return LogSentryMessage(logBuilder, LogLevel.Debug, fingerprint);
        }

        public static LogBuilder WriteSentryInfo(this LogBuilder logBuilder, params string[] fingerprint)
        {
            return LogSentryMessage(logBuilder, LogLevel.Info, fingerprint);
        }

        public static LogBuilder WriteSentryWarn(this LogBuilder logBuilder, params string[] fingerprint)
        {
            return LogSentryMessage(logBuilder, LogLevel.Warn, fingerprint);
        }

        public static LogBuilder WriteSentryError(this LogBuilder logBuilder, params string[] fingerprint)
        {
            return LogSentryMessage(logBuilder, LogLevel.Error, fingerprint);
        }

        private static LogBuilder LogSentryMessage(LogBuilder logBuilder, LogLevel level, string[] fingerprint)
        {
            SentryLogger.Log(level)
                        .CopyLogEvent(logBuilder.LogEventInfo)
                        .SentryFingerprint(fingerprint)
                        .Write();

            return logBuilder.Property("Sentry", null);
        }

        private static LogBuilder CopyLogEvent(this LogBuilder logBuilder, LogEventInfo logEvent)
        {
            return logBuilder.LoggerName(logEvent.LoggerName)
                             .TimeStamp(logEvent.TimeStamp)
                             .Message(logEvent.Message, logEvent.Parameters)
                             .Properties((Dictionary<object, object>)logEvent.Properties)
                             .Exception(logEvent.Exception);
        }
    }
}
