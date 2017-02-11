using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Queue
{
    public class QueueDetailsModule : SonarrRestModuleWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsModule(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage, "queue/details")
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
            GetResourceAll = GetQueue;
        }

        private List<QueueResource> GetQueue()
        {
            var includeSeries = Request.GetBooleanQueryParameter("includeSeries");
            var includeEpisode = Request.GetBooleanQueryParameter("includeEpisode", true);
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);

            var seriesIdQuery = Request.Query.SeriesId;
            var episodeIdsQuery = Request.Query.EpisodeIds;

            if (seriesIdQuery.HasValue)
            {
                return fullQueue.Where(q => q.Series.Id == (int)seriesIdQuery).ToResource(includeSeries, includeEpisode);
            }

            if (episodeIdsQuery.HasValue)
            {
                string episodeIdsValue = episodeIdsQuery.Value.ToString();

                var episodeIds = episodeIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(e => Convert.ToInt32(e))
                                                .ToList();

                return fullQueue.Where(q => episodeIds.Contains(q.Episode.Id)).ToResource(includeSeries, includeEpisode);
            }

            return fullQueue.ToResource(includeSeries, includeEpisode);
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