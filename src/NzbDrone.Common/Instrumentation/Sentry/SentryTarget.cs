using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using SharpRaven;
using SharpRaven.Data;

namespace NzbDrone.Common.Instrumentation.Sentry
{
    [Target("Sentry")]
    public class SentryTarget : TargetWithLayout
    {
        private readonly RavenClient _client;

        private static readonly IDictionary<LogLevel, ErrorLevel> LoggingLevelMap = new Dictionary<LogLevel, ErrorLevel>
        {
            {LogLevel.Debug, ErrorLevel.Debug},
            {LogLevel.Error, ErrorLevel.Error},
            {LogLevel.Fatal, ErrorLevel.Fatal},
            {LogLevel.Info, ErrorLevel.Info},
            {LogLevel.Trace, ErrorLevel.Debug},
            {LogLevel.Warn, ErrorLevel.Warning},
        };

        private readonly SentryDebounce _debounce;
        private bool _unauthorized;


        public SentryTarget(string dsn)
        {
            _client = new RavenClient(new Dsn(dsn), new SonarrJsonPacketFactory(), new SentryRequestFactory(), new MachineNameUserFactory())
            {
                Compression = true,
                Environment = RuntimeInfo.IsProduction ? "production" : "development",
                Release = BuildInfo.Release,
                ErrorOnCapture = OnError
            };


            _client.Tags.Add("osfamily", OsInfo.Os.ToString());
            _client.Tags.Add("runtime", PlatformInfo.PlatformName);
            _client.Tags.Add("culture", Thread.CurrentThread.CurrentCulture.Name);
            _client.Tags.Add("branch", BuildInfo.Branch);
            _client.Tags.Add("version", BuildInfo.Version.ToString());

            _debounce = new SentryDebounce();
        }

        private void OnError(Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null)
            {
                var response = webException.Response as HttpWebResponse;
                var statusCode = response?.StatusCode;
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    _unauthorized = true;
                    _debounce.Clear();
                }
            }

            InternalLogger.Error(ex, "Unable to send error to Sentry");
        }

        private static List<string> GetFingerPrint(LogEventInfo logEvent)
        {
            var fingerPrint = new List<string>
            {
                logEvent.Level.Ordinal.ToString(),
                logEvent.LoggerName
            };

            var ex = logEvent.Exception;

            if (ex != null)
            {
                var exception = ex.GetType().Name;

                if (ex.InnerException != null)
                {
                    exception += ex.InnerException.GetType().Name;
                }

                fingerPrint.Add(exception);
            }

            return fingerPrint;
        }


        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                // don't report non-critical events without exceptions
                if (logEvent.Exception == null || _unauthorized)
                {
                    return;
                }

                var fingerPrint = GetFingerPrint(logEvent);
                if (!_debounce.Allowed(fingerPrint))
                {
                    return;
                }

                var extras = logEvent.Properties.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
                _client.Logger = logEvent.LoggerName;


                var sentryMessage = new SentryMessage(logEvent.Message, logEvent.Parameters);

                var sentryEvent = new SentryEvent(logEvent.Exception)
                {
                    Level = LoggingLevelMap[logEvent.Level],
                    Message = sentryMessage,
                    Extra = extras,
                    Fingerprint =
                    {
                        logEvent.Level.ToString(),
                        logEvent.LoggerName,
                        logEvent.Message
                    }
                };

                if (logEvent.Exception != null)
                {
                    sentryEvent.Fingerprint.Add(logEvent.Exception.GetType().FullName);
                }

                var osName = Environment.GetEnvironmentVariable("OS_NAME");
                var osVersion = Environment.GetEnvironmentVariable("OS_VERSION");
                var runTimeVersion = Environment.GetEnvironmentVariable("RUNTIME_VERSION");


                sentryEvent.Tags.Add("os_name", osName);
                sentryEvent.Tags.Add("os_version", $"{osName} {osVersion}");
                sentryEvent.Tags.Add("runtime_version", $"{PlatformInfo.PlatformName} {runTimeVersion}");

                _client.Capture(sentryEvent);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
