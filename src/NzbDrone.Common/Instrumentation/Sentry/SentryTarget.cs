using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using SharpRaven;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{

    public class SentryUserFactory : ISentryUserFactory
    {
        public SentryUser Create()
        {
            return new SentryUser((string)null);
        }
    }

    [Target("Sentry")]
    public class SentryTarget : TargetWithLayout
    {
        private readonly RavenClient _client;

        /// <summary>
        /// Map of NLog log levels to Raven/Sentry log levels
        /// </summary>
        private static readonly IDictionary<LogLevel, ErrorLevel> LoggingLevelMap = new Dictionary<LogLevel, ErrorLevel>
        {
            {LogLevel.Debug, ErrorLevel.Debug},
            {LogLevel.Error, ErrorLevel.Error},
            {LogLevel.Fatal, ErrorLevel.Fatal},
            {LogLevel.Info, ErrorLevel.Info},
            {LogLevel.Trace, ErrorLevel.Debug},
            {LogLevel.Warn, ErrorLevel.Warning},
        };

        public SentryTarget(string dsn)
        {
            _client = new RavenClient(new Dsn(dsn), new JsonPacketFactory(), new SentryRequestFactory(), new SentryUserFactory())
            {
                Compression = true,
                Environment = RuntimeInfo.IsProduction ? "production" : "development",
                Release = BuildInfo.Release
            };

            _client.Tags.Add("osfamily", OsInfo.Os.ToString());
            _client.Tags.Add("runtime", PlatformInfo.Platform.ToString().ToLower());
            _client.Tags.Add("culture", Thread.CurrentThread.CurrentCulture.Name);
            _client.Tags.Add("branch", BuildInfo.Branch);
            _client.Tags.Add("version", BuildInfo.Version.ToString());
        }


        private List<string> GetFingerPrint(LogEventInfo logEvent)
        {
            var fingerPrint = new List<string>
            {
                logEvent.Level.Ordinal.ToString(),
            };

            var lineNumber = "";

            if (logEvent.StackTrace != null)
            {
                var stackFrame = logEvent.StackTrace.GetFrame(logEvent.UserStackFrameNumber);
                if (stackFrame != null)
                {
                    lineNumber = $"#{stackFrame.GetFileLineNumber()}";
                }
            }

            fingerPrint.Add(logEvent.LoggerName + lineNumber);

            if (logEvent.Exception != null)
            {
                fingerPrint.Add(logEvent.Exception.GetType().Name);
            }

            return fingerPrint;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                // don't report non-critical events without exceptions
                if (logEvent.Exception == null)
                {
                    return;
                }

                var extras = logEvent.Properties.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
                _client.Logger = logEvent.LoggerName;

                var sentryMessage = new SentryMessage(Layout.Render(logEvent));
                var sentryEvent = new SentryEvent(logEvent.Exception)
                {
                    Level = LoggingLevelMap[logEvent.Level],
                    Message = sentryMessage,
                    Extra = extras
                };

                var fingerPrint = GetFingerPrint(logEvent);
                fingerPrint.ForEach(c => sentryEvent.Fingerprint.Add(c));

                sentryEvent.Tags.Add("os_name", Environment.GetEnvironmentVariable("OS_NAME"));
                sentryEvent.Tags.Add("os_version", Environment.GetEnvironmentVariable("OS_VERSION"));
                sentryEvent.Tags.Add("runtime_version", Environment.GetEnvironmentVariable("RUNTIME_VERSION"));

                _client.Capture(sentryEvent);
            }
            catch (Exception e)
            {
                InternalLogger.Error("Unable to send Sentry request: {0}", e.Message);
            }
        }
    }
}
