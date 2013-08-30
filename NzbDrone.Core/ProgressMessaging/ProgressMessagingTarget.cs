using System;
using NLog.Config;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.ProgressMessaging
{

    public class ProgressMessagingTarget : TargetWithLayout, IHandle<ApplicationStartedEvent>, IHandle<ApplicationShutdownRequested>
    {
        private readonly IMessageAggregator _messageAggregator;
        public LoggingRule Rule { get; set; }

        public ProgressMessagingTarget(IMessageAggregator messageAggregator)
        {
            _messageAggregator = messageAggregator;
        }

        public void Register()
        {
            Layout = new SimpleLayout("${callsite:className=false:fileName=false:includeSourcePath=false:methodName=true}");

            Rule = new LoggingRule("*", this);
            Rule.EnableLoggingForLevel(LogLevel.Info);

            LogManager.Configuration.AddTarget("ProgressMessagingLogger", this);
            LogManager.Configuration.LoggingRules.Add(Rule);
            LogManager.ConfigurationReloaded += OnLogManagerOnConfigurationReloaded;
            LogManager.ReconfigExistingLoggers();
        }

        public void UnRegister()
        {
            LogManager.ConfigurationReloaded -= OnLogManagerOnConfigurationReloaded;
            LogManager.Configuration.RemoveTarget("ProgressMessagingLogger");
            LogManager.Configuration.LoggingRules.Remove(Rule);
            LogManager.ReconfigExistingLoggers();
            Dispose();
        }

        private void OnLogManagerOnConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs args)
        {
            Register();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var commandId = MappedDiagnosticsContext.Get("CommandId");

            if (String.IsNullOrWhiteSpace(commandId))
            {
                return;
            }

            var message = new ProgressMessage();
            message.Time = logEvent.TimeStamp;
            message.CommandId = commandId;
            message.Message = logEvent.FormattedMessage;

            _messageAggregator.PublishEvent(new NewProgressMessageEvent(message));
        }

        public void Handle(ApplicationStartedEvent message)
        {
            if (!LogManager.Configuration.LoggingRules.Contains(Rule))
            {
                Register();
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