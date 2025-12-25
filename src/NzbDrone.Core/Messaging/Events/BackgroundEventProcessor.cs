using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Messaging.Events
{
    public class BackgroundEventProcessor : IBackgroundEventProcessor
    {
        private const int MaxParallelism = 50;
        private const int ShutdownTimeoutSeconds = 10;

        private readonly Logger _logger;
        private readonly Channel<QueuedEvent> _eventChannel;
        private readonly Task _processorTask;
        private readonly CancellationTokenSource _shutdownCts;

        public BackgroundEventProcessor(Logger logger)
        {
            _logger = logger;
            _shutdownCts = new CancellationTokenSource();

            _eventChannel = Channel.CreateUnbounded<QueuedEvent>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

            _processorTask = ProcessEventsAsync(_shutdownCts.Token);
        }

        /// <summary>
        /// Queues an event for background processing.
        /// </summary>
        public async ValueTask QueueEventAsync(IEvent @event, object handler, CancellationToken cancellationToken = default)
        {
            try
            {
                await _eventChannel.Writer.WriteAsync(new QueuedEvent(@event, handler, cancellationToken), cancellationToken).ConfigureAwait(false);
            }
            catch (ChannelClosedException)
            {
                _logger.Warn("Cannot queue background event {0} for handler {1} - channel closed", GetEventName(@event.GetType()), handler.GetType().Name);
            }
        }

        private async Task ProcessEventsAsync(CancellationToken cancellationToken)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxParallelism,
                CancellationToken = cancellationToken
            };

            try
            {
                await Parallel.ForEachAsync(
                    _eventChannel.Reader.ReadAllAsync(cancellationToken),
                    parallelOptions,
                    ProcessEventAsync).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Background event processor shutting down");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Background event processor failed critically");
            }
        }

        private async ValueTask ProcessEventAsync(QueuedEvent queuedEvent, CancellationToken processorToken)
        {
            if (queuedEvent.Handler is not IHandleBackgroundAsync<IEvent> handler)
            {
                _logger.Error("Invalid handler type: {0}", queuedEvent.Handler.GetType().Name);
                return;
            }

            var eventName = GetEventName(queuedEvent.Event.GetType());
            var handlerName = handler.GetType().Name;

            try
            {
                _logger.Trace("{0} => {1} (background)", eventName, handlerName);

                var effectiveToken = queuedEvent.CancellationToken.IsCancellationRequested
                    ? processorToken
                    : queuedEvent.CancellationToken;

                await handler.HandleAsync(queuedEvent.Event, effectiveToken).ConfigureAwait(false);

                _logger.Trace("{0} <= {1} (background)", eventName, handlerName);
            }
            catch (OperationCanceledException)
            {
                _logger.Trace("Background handler cancelled: {0}", handlerName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Background handler {0} failed processing {1}", handlerName, eventName);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _logger.Debug("Background event processor shutdown initiated");

            _eventChannel.Writer.Complete();

            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ShutdownTimeoutSeconds));
                await _eventChannel.Reader.Completion.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
                _logger.Debug("Background event queue drained successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.Warn("Background event queue drain timeout - forcing shutdown");
            }

            await _shutdownCts.CancelAsync().ConfigureAwait(false);

            try
            {
                await _processorTask.ConfigureAwait(false);
                _logger.Debug("Background event processor stopped cleanly");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during background processor shutdown");
            }

            _shutdownCts.Dispose();
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

        private record QueuedEvent(IEvent Event, object Handler, CancellationToken CancellationToken);
    }
}
