using NLog.Config;
using NLog;
using NLog.Targets;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Commands.Tracking;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ProgressMessaging
{

    public class ProgressMessageTarget : Target, IHandle<ApplicationStartedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackCommands _trackCommands;
        private static LoggingRule _rule;

        public ProgressMessageTarget(IEventAggregator eventAggregator, ITrackCommands trackCommands)
        {
            _eventAggregator = eventAggregator;
            _trackCommands = trackCommands;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var command = GetCurrentCommand();

            if (IsClientMessage(logEvent, command))
            {
                command.SetMessage(logEvent.FormattedMessage);
                _eventAggregator.PublishEvent(new CommandUpdatedEvent(command));
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