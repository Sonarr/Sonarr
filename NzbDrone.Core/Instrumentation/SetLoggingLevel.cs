using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public interface ISetLoggingLevel
    {
        void Reconfigure();
    }

    public class SetLoggingLevel : ISetLoggingLevel, IHandleAsync<ConfigFileSavedEvent>, IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IConfigFileProvider _configFileProvider;

        public SetLoggingLevel(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public void Reconfigure()
        {
            var minimumLogLevel = LogLevel.FromString(_configFileProvider.LogLevel);

            var rules = LogManager.Configuration.LoggingRules;
            var rollingFileLogger = rules.Single(s => s.Targets.Any(t => t is FileTarget));
            rollingFileLogger.EnableLoggingForLevel(LogLevel.Trace);

            SetMinimumLogLevel(rollingFileLogger, minimumLogLevel);

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

        public void HandleAsync(ApplicationStartedEvent message)
        {
            Reconfigure();
        }
    }
}
