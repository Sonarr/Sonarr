using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Messaging.Events
{
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly Logger _logger;
        private readonly IServiceFactory _serviceFactory;
        private readonly IBackgroundEventProcessor _backgroundProcessor;
        private readonly Dictionary<string, object> _subscriberCache;
        private readonly object _cacheLock = new();

        public EventAggregator(Logger logger, IServiceFactory serviceFactory, IBackgroundEventProcessor backgroundProcessor)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
            _backgroundProcessor = backgroundProcessor;
            _subscriberCache = new Dictionary<string, object>();
        }

        public async Task PublishEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IEvent
        {
            Ensure.That(@event, () => @event).IsNotNull();

            var eventName = GetEventName(typeof(TEvent));
            _logger.Trace("Publishing {0}", eventName);

            var subscribers = GetOrCreateSubscribers<TEvent>(eventName);

            // -> Synchronous handlers (ordered)
            ProcessSyncHandlers(subscribers.SyncHandlers, @event, eventName);

            // ~> Async handlers (parallel)
            await ProcessAsyncHandlersAsync(subscribers.AsyncHandlers, subscribers.GlobalHandlers, @event, eventName, cancellationToken)
                .ConfigureAwait(false);

            // => Background handlers (non-blocking)
            await QueueBackgroundHandlersAsync(subscribers.BackgroundHandlers, @event, eventName, cancellationToken)
                .ConfigureAwait(false);
        }

        private EventSubscribers<TEvent> GetOrCreateSubscribers<TEvent>(string eventName)
            where TEvent : class, IEvent
        {
            lock (_cacheLock)
            {
                if (!_subscriberCache.TryGetValue(eventName, out var cached))
                {
                    cached = new EventSubscribers<TEvent>(_serviceFactory);
                    _subscriberCache[eventName] = cached;
                }

                return (EventSubscribers<TEvent>)cached;
            }
        }

        private void ProcessSyncHandlers<TEvent>(IHandle<TEvent>[] handlers, TEvent @event, string eventName)
            where TEvent : class, IEvent
        {
            foreach (var handler in handlers)
            {
                try
                {
                    var handlerName = handler.GetType().Name;
                    _logger.Trace("{0} -> {1}", eventName, handlerName);

                    handler.Handle(@event);

                    _logger.Trace("{0} <- {1}", eventName, handlerName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "{0} failed while processing [{1}]", handler.GetType().Name, eventName);
                }
            }
        }

        private async Task ProcessAsyncHandlersAsync<TEvent>(
            IHandleAsync<TEvent>[] typedHandlers,
            IHandleAsync<IEvent>[] globalHandlers,
            TEvent @event,
            string eventName,
            CancellationToken cancellationToken)
            where TEvent : class, IEvent
        {
            if (typedHandlers.Length == 0 && globalHandlers.Length == 0)
            {
                return;
            }

            var allHandlers = new List<object>(typedHandlers.Length + globalHandlers.Length);
            allHandlers.AddRange(typedHandlers);
            allHandlers.AddRange(globalHandlers);

            await Parallel.ForEachAsync(allHandlers, cancellationToken, async (handler, ct) =>
            {
                try
                {
                    var handlerName = handler.GetType().Name;
                    _logger.Trace("{0} ~> {1}", eventName, handlerName);

                    if (handler is IHandleAsync<TEvent> typedHandler)
                    {
                        await typedHandler.HandleAsync(@event, ct).ConfigureAwait(false);
                    }
                    else if (handler is IHandleAsync<IEvent> globalHandler)
                    {
                        await globalHandler.HandleAsync(@event, ct).ConfigureAwait(false);
                    }

                    _logger.Trace("{0} <~ {1}", eventName, handlerName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "{0} failed while processing [{1}]", handler.GetType().Name, eventName);
                }
            }).ConfigureAwait(false);
        }

        private async Task QueueBackgroundHandlersAsync<TEvent>(
            IHandleBackgroundAsync<TEvent>[] handlers,
            TEvent @event,
            string eventName,
            CancellationToken cancellationToken)
            where TEvent : class, IEvent
        {
            foreach (var handler in handlers)
            {
                try
                {
                    await _backgroundProcessor.QueueEventAsync(@event, handler, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.Debug("Queuing background handler {0} for [{1}] was cancelled", handler.GetType().Name, eventName);
                    throw;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _logger.Debug("EventAggregator shutdown initiated");
            await _backgroundProcessor.DisposeAsync().ConfigureAwait(false);
        }

        private static string GetEventName(Type eventType)
        {
            if (!eventType.IsGenericType)
            {
                return eventType.Name;
            }

            var genericArgs = string.Join(", ", eventType.GetGenericArguments().Select(t => t.Name));
            return $"{eventType.Name[..eventType.Name.IndexOf('`')]}<{genericArgs}>";
        }

        internal static int GetEventHandleOrder<TEvent>(IHandle<TEvent> eventHandler)
            where TEvent : class, IEvent
        {
            var method = eventHandler.GetType().GetMethod(nameof(eventHandler.Handle), [typeof(TEvent)]);

            if (method == null)
            {
                return (int)EventHandleOrder.Any;
            }

            var attribute = method.GetCustomAttributes(typeof(EventHandleOrderAttribute), true).FirstOrDefault() as EventHandleOrderAttribute;

            return attribute?.EventHandleOrder is { } order ? (int)order : (int)EventHandleOrder.Any;
        }

        private class EventSubscribers<TEvent>(IServiceFactory serviceFactory)
            where TEvent : class, IEvent
        {
            public IHandle<TEvent>[] SyncHandlers { get; } = [.. serviceFactory.BuildAll<IHandle<TEvent>>().OrderBy(GetEventHandleOrder)];
            public IHandleAsync<TEvent>[] AsyncHandlers { get; } = [.. serviceFactory.BuildAll<IHandleAsync<TEvent>>()];
            public IHandleBackgroundAsync<TEvent>[] BackgroundHandlers { get; } = [.. serviceFactory.BuildAll<IHandleBackgroundAsync<TEvent>>()];
            public IHandleAsync<IEvent>[] GlobalHandlers { get; } = [.. serviceFactory.BuildAll<IHandleAsync<IEvent>>()];
        }
    }
}
