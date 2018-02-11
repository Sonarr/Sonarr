using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.Messaging.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly TaskFactory _taskFactory;

        public EventAggregator(Logger logger, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _taskFactory = new TaskFactory();
        }

        public void PublishEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            Ensure.That(@event, () => @event).IsNotNull();

            var eventName = GetEventName(@event.GetType());

            /*
                        int workerThreads;
                        int completionPortThreads;
                        ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

                        int maxCompletionPortThreads;
                        int maxWorkerThreads;
                        ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);


                        int minCompletionPortThreads;
                        int minWorkerThreads;
                        ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);

                        _logger.Warn("Thread pool state WT:{0} PT:{1}  MAXWT:{2} MAXPT:{3} MINWT:{4} MINPT:{5}", workerThreads, completionPortThreads, maxWorkerThreads, maxCompletionPortThreads, minWorkerThreads, minCompletionPortThreads);
            */

            _logger.Trace("Publishing {0}", eventName);


            //call synchronous handlers first.
            var handlers = _serviceFactory.BuildAll<IHandle<TEvent>>()
                                          .OrderBy(GetEventHandleOrder)
                                          .ToList();

            foreach (var handler in handlers)
            {
                try
                {
                    _logger.Trace("{0} -> {1}", eventName, handler.GetType().Name);
                    handler.Handle(@event);
                    _logger.Trace("{0} <- {1}", eventName, handler.GetType().Name);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "{0} failed while processing [{1}]", handler.GetType().Name, eventName);
                }
            }

            foreach (var handler in _serviceFactory.BuildAll<IHandleAsync<IEvent>>())
            {
                var handlerLocal = handler;

                _taskFactory.StartNew(() =>
                {
                    handlerLocal.HandleAsync(@event);
                }, TaskCreationOptions.PreferFairness)
                .LogExceptions();
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

        private int GetEventHandleOrder<TEvent>(IHandle<TEvent> eventHandler) where TEvent : class, IEvent
        {
            // TODO: Convert "Handle" to nameof(eventHandler.Handle) after .net 4.5
            var method = eventHandler.GetType().GetMethod("Handle", new Type[] {typeof(TEvent)});

            if (method == null)
            {
                return (int) EventHandleOrder.Any;
            }

            var attribute = method.GetCustomAttributes(typeof(EventHandleOrderAttribute), true).FirstOrDefault() as EventHandleOrderAttribute;

            if (attribute == null)
            {
                return (int) EventHandleOrder.Any;
            }

            return (int)attribute.EventHandleOrder;
        }
    }
}
