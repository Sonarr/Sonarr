using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using NLog.Config;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;

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
            var logLevel = _configFileProvider.LogLevel;

            var rules = LogManager.Configuration.LoggingRules;
            var rollingFileLogger = rules.Single(s => s.Targets.Any(t => t.Name == "rollingFileLogger"));
            rollingFileLogger.EnableLoggingForLevel(LogLevel.Trace);

            var test = 1;
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
