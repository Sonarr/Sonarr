// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace NzbDrone.Core.Test.Framework
{
    public class ExceptionVerification : Target
    {
        private static List<LogEventInfo> _logs = new List<LogEventInfo>();
        private static List<Type> _inconclusive = new List<Type>();

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Level >= LogLevel.Warn)
            {
                _logs.Add(logEvent);
            }
        }

        internal static void Reset()
        {
            _logs = new List<LogEventInfo>();
            _inconclusive = new List<Type>();
        }

        internal static void AssertNoUnexcpectedLogs()
        {
            ExcpectedFatals(0);
            ExcpectedErrors(0);
            ExcpectedWarns(0);
        }

        private static string GetLogsString(IEnumerable<LogEventInfo> logs)
        {
            string errors = "";
            foreach (var log in logs)
            {
                string exception = "";
                if (log.Exception != null)
                {
                    exception = log.Exception.Message;
                }
                errors += Environment.NewLine + String.Format("[{0}] {1}: {2} [{3}]", log.Level, log.LoggerName, log.FormattedMessage, exception);
            }
            return errors;
        }

        internal static void ExcpectedErrors(int count)
        {
            Excpected(LogLevel.Error, count);
        }

        internal static void ExcpectedFatals(int count)
        {
            Excpected(LogLevel.Fatal, count);
        }

        internal static void ExcpectedWarns(int count)
        {
            Excpected(LogLevel.Warn, count);
        }

        internal static void IgnoreWarns()
        {
            Ignore(LogLevel.Warn);
        }

        internal static void IgnoreErrors()
        {
            Ignore(LogLevel.Error);
        }

        internal static void MarkForInconclusive(Type exception)
        {
            _inconclusive.Add(exception);
        }

        private static void Excpected(LogLevel level, int count)
        {
            var inconclusiveLogs = _logs.Where(l => _inconclusive.Any(c => c == l.Exception.GetType())).ToList();

            var levelLogs = _logs.Except(inconclusiveLogs).Where(l => l.Level == level).ToList();

            if (levelLogs.Count != count)
            {

                var message = String.Format("{0} {1}(s) were expected but {2} were logged.\n\r{3}",
                    count, level, levelLogs.Count, GetLogsString(levelLogs));

                message = "********************************************************************************************************************************\n\r"
                    + message +
                    "\n\r********************************************************************************************************************************";

                Assert.Fail(message);
            }

            if (inconclusiveLogs.Count != 0)
            {
                Assert.Inconclusive(GetLogsString(inconclusiveLogs));
            }

            levelLogs.ForEach(c => _logs.Remove(c));
        }

        private static void Ignore(LogLevel level)
        {
            var levelLogs = _logs.Where(l => l.Level == level).ToList();
            levelLogs.ForEach(c => _logs.Remove(c));
        }
    }
}