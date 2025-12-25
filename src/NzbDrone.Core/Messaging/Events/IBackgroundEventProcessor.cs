using System;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Messaging.Events
{
    public interface IBackgroundEventProcessor : IAsyncDisposable
    {
        ValueTask QueueEventAsync(IEvent @event, object handler, CancellationToken cancellationToken = default);
    }
}
