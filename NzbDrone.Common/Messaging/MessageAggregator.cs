using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace NzbDrone.Common.Messaging
{
    public class MessageAggregator : IMessageAggregator
    {
        private readonly Logger _logger;
        private readonly Func<IEnumerable<IProcessMessage>> _handlerFactory;

        public MessageAggregator(Logger logger, Func<IEnumerable<IProcessMessage>> handlers)
        {
            _logger = logger;
            _handlerFactory = handlers;
        }

        public void PublishEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.Trace("Publishing {0}", @event.GetType().Name);


            var handlers = _handlerFactory().ToList();


            //call synchronous handlers first.
            foreach (var handler in handlers.OfType<IHandle<TEvent>>())
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

            foreach (var handler in handlers.OfType<IHandleAsync<TEvent>>())
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
            _logger.Trace("Publishing {0}", command.GetType().Name);
            var handler = _handlerFactory().OfType<IExecute<TCommand>>().Single();
            _logger.Debug("{0} -> {1}", command.GetType().Name, handler.GetType().Name);
            handler.Execute(command);
            _logger.Debug("{0} <- {1}", command.GetType().Name, handler.GetType().Name);
        }
    }
}
