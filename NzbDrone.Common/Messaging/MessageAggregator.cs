using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Messaging.Events;
using NzbDrone.Common.Messaging.Tracking;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.TPL;

namespace NzbDrone.Common.Messaging
{
    public class MessageAggregator : IMessageAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly ITrackCommands _trackCommands;
        private readonly TaskFactory _taskFactory;

        public MessageAggregator(Logger logger, IServiceFactory serviceFactory, ITrackCommands trackCommands)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _trackCommands = trackCommands;
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(2);
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
                    _logger.Debug("{0} -> {1}", eventName, handler.GetType().Name);
                    handler.Handle(@event);
                    _logger.Debug("{0} <- {1}", eventName, handler.GetType().Name);
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
                    _logger.Debug("{0} ~> {1}", eventName, handlerLocal.GetType().Name);
                    handlerLocal.HandleAsync(@event);
                    _logger.Debug("{0} <~ {1}", eventName, handlerLocal.GetType().Name);
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

        public void PublishCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            Ensure.That(() => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.GetType().Name);

            var trackedCommand = _trackCommands.TrackIfNew(command);

            if (trackedCommand == null)
            {
                _logger.Info("Command is already in progress: {0}", command.GetType().Name);
                return;
            }

            ExecuteCommand<TCommand>(trackedCommand);
        }

        public void PublishCommand(string commandTypeName)
        {
            dynamic command = GetCommand(commandTypeName);
            PublishCommand(command);
        }

        public TrackedCommand PublishCommandAsync<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            Ensure.That(() => command).IsNotNull();

            _logger.Trace("Publishing {0}", command.GetType().Name);

            var existingCommand = _trackCommands.TrackNewOrGet(command);

            if (existingCommand.Existing)
            {
                _logger.Info("Command is already in progress: {0}", command.GetType().Name);
                return existingCommand.TrackedCommand;
            }

            _taskFactory.StartNew(() => ExecuteCommand<TCommand>(existingCommand.TrackedCommand)
                , TaskCreationOptions.PreferFairness)
                .LogExceptions();

            return existingCommand.TrackedCommand;
        }

        public TrackedCommand PublishCommandAsync(string commandTypeName)
        {
            dynamic command = GetCommand(commandTypeName);
            return PublishCommandAsync(command);
        }

        private dynamic GetCommand(string commandTypeName)
        {
            var commandType = _serviceFactory.GetImplementations(typeof(ICommand))
                .Single(c => c.FullName.Equals(commandTypeName, StringComparison.InvariantCultureIgnoreCase));

            return Json.Deserialize("{}", commandType);
        }

        private void ExecuteCommand<TCommand>(TrackedCommand trackedCommand) where TCommand : class, ICommand
        {
            var command = (TCommand)trackedCommand.Command;

            var handlerContract = typeof(IExecute<>).MakeGenericType(command.GetType());
            var handler = (IExecute<TCommand>)_serviceFactory.Build(handlerContract);

            _logger.Debug("{0} -> {1}", command.GetType().Name, handler.GetType().Name);

            var sw = Stopwatch.StartNew();

            try
            {
                MappedDiagnosticsContext.Set("CommandId", trackedCommand.Command.CommandId);

                PublishEvent(new CommandStartedEvent(trackedCommand));
                handler.Execute(command);
                sw.Stop();

                _trackCommands.Completed(trackedCommand, sw.Elapsed);
                PublishEvent(new CommandCompletedEvent(trackedCommand));
            }
            catch (Exception e)
            {
                _trackCommands.Failed(trackedCommand, e);
                PublishEvent(new CommandFailedEvent(trackedCommand, e));
                throw;
            }
            finally
            {
                PublishEvent(new CommandExecutedEvent(trackedCommand));
            }

            _logger.Debug("{0} <- {1} [{2}]", command.GetType().Name, handler.GetType().Name, sw.Elapsed.ToString(""));
        }
    }
}
