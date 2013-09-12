using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Messaging.Tracking;
using NzbDrone.Core.ProgressMessaging;

namespace NzbDrone.Core.Messaging
{
    public class MessageAggregator : IMessageAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly ITrackCommands _trackCommands;
        private readonly TaskFactory _taskFactory;

        public MessageAggregator(Logger logger, IServiceFactory serviceFactory, ITrackCommands trackCommands)
        {
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(3);

            _logger = logger;
            _serviceFactory = serviceFactory;
            _trackCommands = trackCommands;
            _taskFactory = new TaskFactory(scheduler);
        }

        public void PublishEvent<TEvent>(TEvent @event) where TEvent : class ,IEvent
        {
            Ensure.That(() => @event).IsNotNull();

            var eventName = GetEventName(@event.GetType());

            _logger.Trace("Publishing {0}", eventName);

            //call synchronous handlers first.
            foreach (var handler in _serviceFactory.BuildAll<IHandle<TEvent>>())
            {
                try
                {
                    _logger.Trace("{0} -> {1}", eventName, handler.GetType().Name);
                    handler.Handle(@event);
                    _logger.Trace("{0} <- {1}", eventName, handler.GetType().Name);
                }
                catch (Exception e)
                {
                    _logger.ErrorException(string.Format("{0} failed while processing [{1}]", handler.GetType().Name, eventName), e);
                }
            }

            foreach (var handler in _serviceFactory.BuildAll<IHandleAsync<TEvent>>())
            {
                var handlerLocal = handler;

                _taskFactory.StartNew(() =>
                {
                    _logger.Trace("{0} ~> {1}", eventName, handlerLocal.GetType().Name);
                    handlerLocal.HandleAsync(@event);
                    _logger.Trace("{0} <~ {1}", eventName, handlerLocal.GetType().Name);
                }, TaskCreationOptions.PreferFairness)
                .LogExceptions();
            }
        }

        private static string GetEventName(Type eventType)
        {
            if (!eventType.IsGenericType)
            {
                return eventType.Name;
            }

            return string.Format("{0}<{1}>", eventType.Name.Remove(eventType.Name.IndexOf('`')), eventType.GetGenericArguments()[0].Name);
        }

        public void PublishCommand<TCommand>(TCommand command) where TCommand : Command
        {
            Ensure.That(() => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.GetType().Name);

            if (_trackCommands.FindExisting(command) != null)
            {
                _logger.Debug("Command is already in progress: {0}", command.GetType().Name);
                return;
            }

            _trackCommands.Store(command);

            ExecuteCommand<TCommand>(command);
        }

        public void PublishCommand(string commandTypeName)
        {
            dynamic command = GetCommand(commandTypeName);
            PublishCommand(command);
        }

        public Command PublishCommandAsync<TCommand>(TCommand command) where TCommand : Command
        {
            Ensure.That(() => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.GetType().Name);

            var existingCommand = _trackCommands.FindExisting(command);

            if (existingCommand != null)
            {
                _logger.Debug("Command is already in progress: {0}", command.GetType().Name);
                return existingCommand;
            }

            _trackCommands.Store(command);

            _taskFactory.StartNew(() => ExecuteCommand<TCommand>(command)
                , TaskCreationOptions.PreferFairness)
                .LogExceptions();

            return command;
        }

        public Command PublishCommandAsync(string commandTypeName)
        {
            dynamic command = GetCommand(commandTypeName);
            return PublishCommandAsync(command);
        }

        private dynamic GetCommand(string commandTypeName)
        {
            var commandType = _serviceFactory.GetImplementations(typeof(Command))
                .Single(c => c.FullName.Equals(commandTypeName, StringComparison.InvariantCultureIgnoreCase));

            return Json.Deserialize("{}", commandType);
        }

        private void ExecuteCommand<TCommand>(Command command) where TCommand : Command
        {
            var handlerContract = typeof(IExecute<>).MakeGenericType(command.GetType());
            var handler = (IExecute<TCommand>)_serviceFactory.Build(handlerContract);

            _logger.Trace("{0} -> {1}", command.GetType().Name, handler.GetType().Name);

            try
            {
                _trackCommands.Start(command);
                PublishEvent(new CommandUpdatedEvent(command));

                if (!MappedDiagnosticsContext.Contains("CommandId") && command.SendUpdatesToClient)
                {
                    MappedDiagnosticsContext.Set("CommandId", command.Id.ToString());
                }

                handler.Execute((TCommand)command);
                _trackCommands.Completed(command);
                PublishEvent(new CommandUpdatedEvent(command));

            }
            catch (Exception e)
            {
                _trackCommands.Failed(command, e);
                PublishEvent(new CommandUpdatedEvent(command));
                throw;
            }

            PublishEvent(new CommandExecutedEvent(command));
            PublishEvent(new CommandUpdatedEvent(command));

            _logger.Trace("{0} <- {1} [{2}]", command.GetType().Name, handler.GetType().Name, command.Runtime.ToString(""));
        }
    }
}
