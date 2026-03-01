using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Queue
{
    [V5ApiController("queue/details")]
    public class QueueDetailsController : RestControllerWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsController(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
        }

        [NonAction]
        public override ActionResult<QueueResource> GetResourceByIdWithErrorHandler(int id)
        {
            return base.GetResourceByIdWithErrorHandler(id);
        }

        protected override QueueResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<QueueResource> GetQueue(int? seriesId, [FromQuery]List<int> episodeIds, [FromQuery] QueueSubresource[]? includeSubresources = null)
        {
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);
            var includeSeries = includeSubresources.Contains(QueueSubresource.Series);
            var includeEpisodes = includeSubresources.Contains(QueueSubresource.Episodes);

            if (seriesId.HasValue)
            {
                return fullQueue.Where(q => q.Series?.Id == seriesId).ToResource(includeSeries, includeEpisodes);
            }

            if (episodeIds.Any())
            {
                return fullQueue.Where(q => q.Episodes.Any() &&
                                            episodeIds.IntersectBy(e => e, q.Episodes, e => e.Id, null).Any())
                    .ToResource(includeSeries, includeEpisodes);
            }

            return fullQueue.ToResource(includeSeries, includeEpisodes);
        }

        [NonAction]
        public void Handle(QueueUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        [NonAction]
        public void Handle(PendingReleasesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
