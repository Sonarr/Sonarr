using System;
using NLog.Config;
using NLog;
using NLog.Targets;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ProgressMessaging
{
    public class ProgressMessageTarget : Target, IHandle<ApplicationStartedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private static LoggingRule _rule;

        private const string REENTRY_LOCK = "ProgressMessagingLock";

        public ProgressMessageTarget(IEventAggregator eventAggregator, IManageCommandQueue commandQueueManager)
        {
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (!ReentryPreventionCheck()) return;

            var command = GetCurrentCommand();

            if (IsClientMessage(logEvent, command))
            {
                _commandQueueManager.SetMessage(command, logEvent.FormattedMessage);
                _eventAggregator.PublishEvent(new CommandUpdatedEvent(command));
            }

            MappedDiagnosticsContext.Remove(REENTRY_LOCK);
        }

        private CommandModel GetCurrentCommand()
        {
            var commandId = MappedDiagnosticsContext.Get("CommandId");

            if (String.IsNullOrWhiteSpace(commandId))
            {
                return null;
            }

            return _commandQueueManager.Get(Convert.ToInt32(commandId));
        }

        private bool IsClientMessage(LogEventInfo logEvent, CommandModel command)
        {
            if (command == null || !command.Body.SendUpdatesToClient)
            {
                return false;
            }

            return logEvent.Properties.ContainsKey("Status");
        }

        private bool ReentryPreventionCheck()
        {
            var reentryLock = MappedDiagnosticsContext.Get(REENTRY_LOCK);
            var commandId = MappedDiagnosticsContext.Get("CommandId");

            if (reentryLock.IsNullOrWhiteSpace() || reentryLock != commandId)
            {
                MappedDiagnosticsContext.Set(REENTRY_LOCK, MappedDiagnosticsContext.Get("CommandId"));
                return true;
            }

            return false;
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
