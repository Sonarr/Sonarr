using System;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Messaging.Events
{
    public interface IEventAggregator : IAsyncDisposable
    {
        Task PublishEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IEvent;
    }
}
