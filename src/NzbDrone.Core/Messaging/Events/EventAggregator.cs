using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, object> _eventSubscribers;

        private class EventSubscribers<TEvent>
            where TEvent : class, IEvent
        {
            public IHandle<TEvent>[] _syncHandlers;
            public IHandleAsync<TEvent>[] _asyncHandlers;
            public IHandleAsync<IEvent>[] _globalHandlers;

            public EventSubscribers(IServiceFactory serviceFactory)
            {
                _syncHandlers = serviceFactory.BuildAll<IHandle<TEvent>>()
                                              .OrderBy(GetEventHandleOrder)
                                              .ToArray();

                _globalHandlers = serviceFactory.BuildAll<IHandleAsync<IEvent>>()
                                              .ToArray();

                _asyncHandlers = serviceFactory.BuildAll<IHandleAsync<TEvent>>()
                                               .ToArray();
            }
        }

        public EventAggregator(Logger logger, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _taskFactory = new TaskFactory();
            _eventSubscribers = new Dictionary<string, object>();
        }

        public void PublishEvent<TEvent>(TEvent @event)
            where TEvent : class, IEvent
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

            EventSubscribers<TEvent> subscribers;
            lock (_eventSubscribers)
            {
                object target;
                if (!_eventSubscribers.TryGetValue(eventName, out target))
                {
                    _eventSubscribers[eventName] = target = new EventSubscribers<TEvent>(_serviceFactory);
                }

                subscribers = target as EventSubscribers<TEvent>;
            }

            //call synchronous handlers first.
            var handlers = subscribers._syncHandlers;
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

            foreach (var handler in subscribers._globalHandlers)
            {
                var handlerLocal = handler;

                _taskFactory.StartNew(() =>
                {
                    handlerLocal.HandleAsync(@event);
                }, TaskCreationOptions.PreferFairness)
                .LogExceptions();
            }

            foreach (var handler in subscribers._asyncHandlers)
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

        internal static int GetEventHandleOrder<TEvent>(IHandle<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            var method = eventHandler.GetType().GetMethod(nameof(eventHandler.Handle), new Type[] { typeof(TEvent) });

            if (method == null)
            {
                return (int)EventHandleOrder.Any;
            }

            var attribute = method.GetCustomAttributes(typeof(EventHandleOrderAttribute), true).FirstOrDefault() as EventHandleOrderAttribute;

            if (attribute == null)
            {
                return (int)EventHandleOrder.Any;
            }

            return (int)attribute.EventHandleOrder;
        }
    }
}
