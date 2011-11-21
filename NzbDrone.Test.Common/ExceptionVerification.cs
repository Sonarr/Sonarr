// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace NzbDrone.Test.Common
{
    public class ExceptionVerification : Target
    {
        private static List<LogEventInfo> _logs = new List<LogEventInfo>();

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Level >= LogLevel.Warn)
            {
                _logs.Add(logEvent);
            }
        }

        public static void Reset()
        {
            _logs = new List<LogEventInfo>();
        }

        public static void AssertNoUnexcpectedLogs()
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
                    exception = String.Format("[{0}: {1}]", log.Exception.GetType(), log.Exception.Message);
                }

                errors += Environment.NewLine + String.Format("[{0}] {1}: {2} {3}", log.Level, log.LoggerName, log.FormattedMessage, exception);
            }
            return errors;
        }

        public static void ExcpectedErrors(int count)
        {
            Excpected(LogLevel.Error, count);
        }

        public static void ExcpectedFatals(int count)
        {
            Excpected(LogLevel.Fatal, count);
        }

        public static void ExcpectedWarns(int count)
        {
            Excpected(LogLevel.Warn, count);
        }

        public static void IgnoreWarns()
        {
            Ignore(LogLevel.Warn);
        }

        public static void IgnoreErrors()
        {
            Ignore(LogLevel.Error);
        }

        public static void MarkInconclusive(Type exception)
        {
            var inconclusiveLogs = _logs.Where(l => l.Exception != null && l.Exception.GetType() == exception).ToList();

            if (inconclusiveLogs.Count != 0)
            {
                inconclusiveLogs.ForEach(c => _logs.Remove(c));
                Assert.Inconclusive(GetLogsString(inconclusiveLogs));

            }
        }

        private static void Excpected(LogLevel level, int count)
        {
            var levelLogs = _logs.Where(l => l.Level == level).ToList();

            if (levelLogs.Count != count)
            {

                var message = String.Format("{0} {1}(s) were expected but {2} were logged.\n\r{3}",
                    count, level, levelLogs.Count, GetLogsString(levelLogs));

                message = "\n\r****************************************************************************************\n\r"
                    + message +
                    "\n\r****************************************************************************************";

                Assert.Fail(message);
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