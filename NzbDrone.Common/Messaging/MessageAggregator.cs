using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Common.Messaging
{
    [Singleton]
    public class MessageAggregator : IMessageAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;

        public MessageAggregator(Logger logger, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
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
                Task.Factory.StartNew(() =>
                {
                    _logger.Debug("{0} ~> {1}", eventName, handlerLocal.GetType().Name);
                    handlerLocal.HandleAsync(@event);
                    _logger.Debug("{0} <~ {1}", eventName, handlerLocal.GetType().Name);
                });
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

            var handlerContract = typeof(IExecute<>).MakeGenericType(command.GetType());

            _logger.Trace("Publishing {0}", command.GetType().Name);

            var handler = _serviceFactory.Build(handlerContract);

            _logger.Debug("{0} -> {1}", command.GetType().Name, handler.GetType().Name);

            try
            {
                handlerContract.GetMethod("Execute").Invoke(handler, new object[] { command });
                PublishEvent(new CommandCompletedEvent(command));
            }
            catch (TargetInvocationException e)
            {
                PublishEvent(new CommandFailedEvent(command, e));

                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
            finally
            {
                PublishEvent(new CommandExecutedEvent(command));
            }

            _logger.Debug("{0} <- {1}", command.GetType().Name, handler.GetType().Name);
        }

        public void PublishCommand(string commandTypeName)
        {
            var commandType = _serviceFactory.GetImplementations(typeof(ICommand))
                .Single(c => c.FullName.Equals(commandTypeName, StringComparison.InvariantCultureIgnoreCase));

            //json.net is better at creating objects
            var command = Json.Deserialize("{}", commandType);
            PublishCommand((ICommand)command);
        }
    }
}
