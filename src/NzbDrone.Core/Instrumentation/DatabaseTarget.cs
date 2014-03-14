using System;
using System.Data.SQLite;
using NLog.Common;
using NLog.Config;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{

    public class DatabaseTarget : TargetWithLayout, IHandle<ApplicationShutdownRequested>
    {
        private readonly ILogRepository _repository;

        public DatabaseTarget(ILogRepository repository)
        {
            _repository = repository;
        }

        public void Register()
        {
            Layout = new SimpleLayout("${callsite:className=false:fileName=false:includeSourcePath=false:methodName=true}");

            Rule = new LoggingRule("*", LogLevel.Info, this);

            LogManager.Configuration.AddTarget("DbLogger", this);
            LogManager.Configuration.LoggingRules.Add(Rule);
            LogManager.ConfigurationReloaded += OnLogManagerOnConfigurationReloaded;
            LogManager.ReconfigExistingLoggers();
        }

        public void UnRegister()
        {
            LogManager.ConfigurationReloaded -= OnLogManagerOnConfigurationReloaded;
            LogManager.Configuration.RemoveTarget("DbLogger");
            LogManager.Configuration.LoggingRules.Remove(Rule);
            LogManager.ReconfigExistingLoggers();
            Dispose();
        }

        private void OnLogManagerOnConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs args)
        {
            Register();
        }

        public LoggingRule Rule { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var log = new Log();
            log.Time = logEvent.TimeStamp;
            log.Message = logEvent.FormattedMessage;
            log.Method = Layout.Render(logEvent);

            log.Logger = logEvent.LoggerName;

            if (log.Logger.StartsWith("NzbDrone."))
            {
                log.Logger = log.Logger.Remove(0, 9);
            }

            if (logEvent.Exception != null)
            {
                if (String.IsNullOrWhiteSpace(log.Message))
                {
                    log.Message = logEvent.Exception.Message;
                }
                else
                {
                    log.Message += ": " + logEvent.Exception.Message;
                }


                log.Exception = logEvent.Exception.ToString();
                log.ExceptionType = logEvent.Exception.GetType().ToString();
            }

            
            log.Level = logEvent.Level.Name;

            try
            {
                _repository.Insert(log);
            }
            catch (SQLiteException ex)
            {
                InternalLogger.Error("Unable to save log event to database: {0}", ex);
                throw;
            }
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            if (LogManager.Configuration.LoggingRules.Contains(Rule))
            {
                UnRegister();
            }
        }
    }
}