using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Movies;
using NzbDrone.Api.Series;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;

namespace NzbDrone.Api.Queue
{
    public class QueueModule : NzbDroneRestModuleWithSignalR<QueueResource, Core.Queue.Queue>,
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
            return GetQueueItems().Select(h =>
                                              {
                                                  var resource = h.InjectTo<QueueResource>();
                                                  if (h.Media is NzbDrone.Core.Tv.Series)
                                                  {
                                                      resource.Series = h.Media.InjectTo<SeriesResource>();
                                                  }
                                                  else if (h.Media is Movie)
                                                  {
                                                      resource.Movie = h.Media.InjectTo<MoviesResource>();
                                                  }
                                                  return resource;
                                              }
                                          ).ToList();
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