using System;
using NLog;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptron;
using NzbDrone.Common.Exceptron.Configuration;

namespace NzbDrone.Common.Instrumentation
{
    /// <summary>
    /// <see cref="NLog"/> target for exceptron. Allows you to automatically report all
    /// exceptions logged to Nlog/>
    /// </summary>
    [Target("Exceptron")]
    public class ExceptronTarget : Target
    {
        /// <summary>
        /// <see cref="ExceptronClient"/> instance that Nlog Target uses to report the exceptions.
        /// </summary>
        public IExceptronClient ExceptronClient { get; internal set; }

        protected override void InitializeTarget()
        {
            var config = new ExceptronConfiguration
                {
                    ApiKey = "d64e0a72845d495abc625af3a27cf5f5",
                    IncludeMachineName = true,
                };

            if (RuntimeInfoBase.IsProduction)
            {
                config.ApiKey = "82c0f66dd2d64d1480cc88b551c9bdd8";
            }

            ExceptronClient = new ExceptronClient(config, BuildInfo.Version);
        }


        /// <summary>
        /// String that identifies the active user
        /// </summary>
        public Layout UserId { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null || logEvent.Exception == null || logEvent.Exception.ExceptronShouldIgnore()) return;

            try
            {
                var exceptionData = new ExceptionData
                {
                    Exception = logEvent.Exception,
                    Component = logEvent.LoggerName,
                    Message = logEvent.FormattedMessage,
                };

                if (UserId != null)
                {
                    exceptionData.UserId = UserId.Render(logEvent);
                }

                if (logEvent.Level <= LogLevel.Info)
                {
                    exceptionData.Severity = ExceptionSeverity.None;
                }
                else if (logEvent.Level <= LogLevel.Warn)
                {
                    exceptionData.Severity = ExceptionSeverity.Warning;
                }
                else if (logEvent.Level <= LogLevel.Error)
                {
                    exceptionData.Severity = ExceptionSeverity.Error;
                }
                else if (logEvent.Level <= LogLevel.Fatal)
                {
                    exceptionData.Severity = ExceptionSeverity.Fatal;
                }

                ExceptronClient.SubmitException(exceptionData);
            }
            catch (Exception e)
            {
                InternalLogger.Warn("Unable to report exception. {0}", e);
            }
        }
    }
}