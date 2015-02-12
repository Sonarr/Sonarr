using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public class ReconfigureLogging : IHandleAsync<ConfigFileSavedEvent>,
                                      IHandle<ApplicationStartedEvent>
    {
        private readonly IConfigFileProvider _configFileProvider;

        public ReconfigureLogging(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public void Reconfigure()
        {
            var minimumLogLevel = LogLevel.FromString(_configFileProvider.LogLevel);

            var rules = LogManager.Configuration.LoggingRules;

            //Console
            var consoleLoggerRule = rules.Single(s => s.Targets.Any(t => t is ColoredConsoleTarget));
            consoleLoggerRule.EnableLoggingForLevel(LogLevel.Trace);

            SetMinimumLogLevel(consoleLoggerRule, minimumLogLevel);

            //Log Files
            var rollingFileLoggerRule = rules.Single(s => s.Targets.Any(t => t is NzbDroneFileTarget));
            rollingFileLoggerRule.EnableLoggingForLevel(LogLevel.Trace);
            
            SetMinimumLogLevel(rollingFileLoggerRule, minimumLogLevel);
            SetMaxArchiveFiles(rollingFileLoggerRule, minimumLogLevel);

            LogManager.ReconfigExistingLoggers();
        }

        private void SetMinimumLogLevel(LoggingRule rule, LogLevel minimumLogLevel)
        {
            foreach (var logLevel in GetLogLevels())
            {
                if (logLevel < minimumLogLevel)
                {
                    rule.DisableLoggingForLevel(logLevel);
                }

                else
                {
                    rule.EnableLoggingForLevel(logLevel);
                }
            }
        }

        private void SetMaxArchiveFiles(LoggingRule rule, LogLevel minimumLogLevel)
        {
            var target = rule.Targets.Single(t => t is NzbDroneFileTarget) as NzbDroneFileTarget;

            if (target == null) return;

            if (minimumLogLevel >= LogLevel.Info)
            {
                target.MaxArchiveFiles = 5;
            }

            else
            {
                target.MaxArchiveFiles = 50;
            }
        }

        private List<LogLevel> GetLogLevels()
        {
            return new List<LogLevel>
                       {
                           LogLevel.Trace,
                           LogLevel.Debug,
                           LogLevel.Info,
                           LogLevel.Warn,
                           LogLevel.Error,
                           LogLevel.Fatal
                       };
        }

        public void HandleAsync(ConfigFileSavedEvent message)
        {
            Reconfigure();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            Reconfigure();
        }
    }
}
