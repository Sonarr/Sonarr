using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace NzbDrone.Common.Eventing
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Logger _logger;
        private readonly Func<IEnumerable<IHandle>> _handlers;

        public EventAggregator(Logger logger, Func<IEnumerable<IHandle>> handlers)
        {
            _logger = logger;
            _handlers = handlers;
        }

        public void Publish<TEvent>(TEvent message) where TEvent : IEvent
        {
            _logger.Trace("Publishing {0}", message.GetType().Name);

            //call synchronous handlers first.
            foreach (var handler in _handlers().OfType<IHandle<TEvent>>())
            {
                _logger.Debug("{0} -> {1}", message.GetType().Name, handler.GetType().Name);
                handler.Handle(message);
                _logger.Debug("{0} <- {1}", message.GetType().Name, handler.GetType().Name);
            }

            foreach (var handler in _handlers().OfType<IHandleAsync<TEvent>>())
            {
                var handlerLocal = handler;
                Task.Factory.StartNew(() =>
                    {
                        _logger.Debug("{0} ~> {1}", message.GetType().Name, handlerLocal.GetType().Name);
                        handlerLocal.HandleAsync(message);
                        _logger.Debug("{0} <~ {1}", message.GetType().Name, handlerLocal.GetType().Name);
                    });
            }
        }
    }
}
