using System.Collections.Generic;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;

namespace NzbDrone.Api.Queue
{
    public class QueueModule : NzbDroneRestModuleWithSignalR<QueueResource, Core.Queue.Queue>,
                               IHandle<UpdateQueueEvent>
    {
        private readonly IQueueService _queueService;

        public QueueModule(ICommandExecutor commandExecutor, IQueueService queueService)
            : base(commandExecutor)
        {
            _queueService = queueService;
            GetResourceAll = GetQueue;
        }

        private List<QueueResource> GetQueue()
        {
            return ToListResource(_queueService.GetQueue);
        }

        public void Handle(UpdateQueueEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}