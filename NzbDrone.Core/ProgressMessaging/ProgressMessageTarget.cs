using NLog.Config;
using NLog;
using NLog.Targets;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Tracking;

namespace NzbDrone.Core.ProgressMessaging
{

    public class ProgressMessageTarget : Target, IHandle<ApplicationStartedEvent>
    {
        private readonly IMessageAggregator _messageAggregator;
        private readonly ITrackCommands _trackCommands;
        private static LoggingRule _rule;

        public ProgressMessageTarget(IMessageAggregator messageAggregator, ITrackCommands trackCommands)
        {
            _messageAggregator = messageAggregator;
            _trackCommands = trackCommands;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var command = GetCurrentCommand();

            if (IsClientMessage(logEvent, command))
            {
                command.SetMessage(logEvent.FormattedMessage);
                _messageAggregator.PublishEvent(new CommandUpdatedEvent(command));
            }
        }


        private Command GetCurrentCommand()
        {
            var commandId = MappedDiagnosticsContext.Get("CommandId");

            if (string.IsNullOrWhiteSpace(commandId))
            {
                return null;
            }

            return _trackCommands.GetById(commandId);
        }

        private bool IsClientMessage(LogEventInfo logEvent, Command command)
        {
            if (command == null || !command.SendUpdatesToClient)
            {
                return false;
            }

            return logEvent.Properties.ContainsKey("Status");
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _rule = new LoggingRule("*", LogLevel.Trace, this);

            LogManager.Configuration.AddTarget("ProgressMessagingLogger", this);
            LogManager.Configuration.LoggingRules.Add(_rule);
            LogManager.ReconfigExistingLoggers();
        }
    }
}