using System;
using System.Linq;
using Nancy.Responses;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Queue
{
    public class QueueStatusModule : SonarrRestModuleWithSignalR<QueueStatusResource, NzbDrone.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Debouncer _broadcastDebounce;


        public QueueStatusModule(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage, "queue/status")
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;

            _broadcastDebounce = new Debouncer(BroadcastChange, TimeSpan.FromSeconds(5));


            Get["/"] = x => GetQueueStatusResponse();
        }

        private JsonResponse<QueueStatusResource> GetQueueStatusResponse()
        {
            return GetQueueStatus().AsResponse();
        }

        private QueueStatusResource GetQueueStatus()
        {
            _broadcastDebounce.Pause();

            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();

            var resource = new QueueStatusResource
            {
                Count = queue.Count + pending.Count,
                Errors = queue.Any(q => q.TrackedDownloadStatus.Equals("Error", StringComparison.InvariantCultureIgnoreCase)),
                Warnings = queue.Any(q => q.TrackedDownloadStatus.Equals("Warning", StringComparison.InvariantCultureIgnoreCase))
            };

            _broadcastDebounce.Resume();

            return resource;
        }

        private void BroadcastChange()
        {
            BroadcastResourceChange(ModelAction.Updated, GetQueueStatus());
        }

        public void Handle(QueueUpdatedEvent message)
        {
            _broadcastDebounce.Execute();
        }

        public void Handle(PendingReleasesUpdatedEvent message)
        {
            _broadcastDebounce.Execute();
        }
    }
}
