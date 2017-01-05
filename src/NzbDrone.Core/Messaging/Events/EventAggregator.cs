using System;
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
                    _logger.Error(e, "{0} failed while processing [{1}]", handler.GetType().Name, eventName);
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
    }
}
