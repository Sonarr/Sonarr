using System;
using System.Linq;
using System.Collections.Generic;
using NLog;
using NLog.Targets;
using MbUnit.Framework;

namespace NzbDrone.Core.Test.Framework
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

        internal static void Reset()
        {
            _logs = new List<LogEventInfo>();
        }

        internal static void AssertNoError()
        {
            if (_logs.Count != 0)
            {
                string errors = GetLogsString(_logs);

                var message = String.Format("{0} unexpected Fatal/Error/Warning were logged during execution.\n\r Use ExceptionVerification.Excpected methods if errors are excepted for this test.{1}{2}",
                    _logs.Count,
                    Environment.NewLine,
                    errors);

                Assert.Fail(message);
            }
        }

        private static string GetLogsString(IEnumerable<LogEventInfo> logs)
        {
            string errors = "";
            foreach (var log in logs)
            {
                string exception = "";
                if (log.Exception != null)
                {
                    exception = log.Exception.ToString();
                }
                errors += Environment.NewLine + String.Format("[{0}] {1}: {2} {3}", log.Level, log.LoggerName, log.FormattedMessage, exception);
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

        private static void Excpected(LogLevel level, int count)
        {
            var levelLogs = _logs.Where(l => l.Level == level).ToList();

            if (levelLogs.Count != count)
            {
                var message = String.Format("{0} {1}(s) were expected but {2} were logged.\n\r{3}",
                    count, level, _logs.Count, GetLogsString(levelLogs));

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