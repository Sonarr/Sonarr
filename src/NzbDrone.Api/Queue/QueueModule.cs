using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace NzbDrone.Api.Queue
{
    public class QueueModule : SonarrRestModuleWithSignalR<QueueResource, Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueModule(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
            GetResourceAll = GetQueue;
        }

        private List<QueueResource> GetQueue()
        {
            return GetQueueItems().ToResource();
        }

        private IEnumerable<Core.Queue.Queue> GetQueueItems()
        {
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();

            return queue.Concat(pending);
        }

        public void Handle(QueueUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        public void Handle(PendingReleasesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}