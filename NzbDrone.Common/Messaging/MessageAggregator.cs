using System;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace NzbDrone.Common.Messaging
{
    public class MessageAggregator : IMessageAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;

        public MessageAggregator(Logger logger, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
        }

        public void PublishEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.Trace("Publishing {0}", @event.GetType().Name);

            //call synchronous handlers first.
            foreach (var handler in _serviceFactory.BuildAll<IHandle<TEvent>>())
            {
                try
                {
                    _logger.Debug("{0} -> {1}", @event.GetType().Name, handler.GetType().Name);
                    handler.Handle(@event);
                    _logger.Debug("{0} <- {1}", @event.GetType().Name, handler.GetType().Name);
                }
                catch (Exception e)
                {
                    _logger.ErrorException(string.Format("{0} failed while processing [{1}]", handler.GetType().Name, @event.GetType().Name), e);
                }
            }

            foreach (var handler in _serviceFactory.BuildAll<IHandleAsync<TEvent>>())
            {
                var handlerLocal = handler;
                Task.Factory.StartNew(() =>
                {
                    _logger.Debug("{0} ~> {1}", @event.GetType().Name, handlerLocal.GetType().Name);
                    handlerLocal.HandleAsync(@event);
                    _logger.Debug("{0} <~ {1}", @event.GetType().Name, handlerLocal.GetType().Name);
                });
            }
        }


        public void PublishCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handlerContract = typeof(IExecute<>).MakeGenericType(command.GetType());

            _logger.Trace("Publishing {0}", command.GetType().Name);

            var handler = _serviceFactory.Build(handlerContract);

            _logger.Debug("{0} -> {1}", command.GetType().Name, handler.GetType().Name);
            
            try
            {
                handlerContract.GetMethod("Execute").Invoke(handler, new object[] { command });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }

            _logger.Debug("{0} <- {1}", command.GetType().Name, handler.GetType().Name);
        }
    }
}
